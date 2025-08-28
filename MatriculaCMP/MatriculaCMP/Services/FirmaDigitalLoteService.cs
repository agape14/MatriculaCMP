using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using static MatriculaCMP.Shared.FirmaDigitalDTO;

namespace MatriculaCMP.Services
{
    public class FirmaDigitalLoteService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FirmaDigitalLoteService> _logger;

        private readonly FirmaDigitalService _singleService; // reutilizar token, descarga y post

        public FirmaDigitalLoteService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<FirmaDigitalLoteService> logger, FirmaDigitalService singleService)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _singleService = singleService;
        }

        public async Task<UploadResponse> FirmarLoteAsync(IEnumerable<int> solicitudIds, int tipoDocumentoFirmado)
        {
            var ids = solicitudIds?.Distinct().ToList() ?? new List<int>();
            if (!ids.Any())
                return new UploadResponse { codigoFirma = -1, descripcion = "No hay documentos a firmar" };

            var rutaArchivos = _config["FirmaDigital:RutaArchivos"] ?? "wwwroot/firmas_digitales/";
            Directory.CreateDirectory(rutaArchivos);

            string tempDir = Path.Combine(Path.GetTempPath(), "cmp_firmas_lote_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                foreach (var id in ids)
                {
                    // Seleccionar el documento actual más firmado
                    var baseName = $"documento_{id}";
                    var candidatos = Directory.GetFiles(rutaArchivos, baseName + "*.pdf");
                    string? elegido = candidatos
                        .OrderByDescending(p => Path.GetFileNameWithoutExtension(p).Count(c => c == 'F'))
                        .FirstOrDefault();
                    if (string.IsNullOrEmpty(elegido))
                    {
                        // fallback al documento de trabajo base
                        elegido = Path.Combine(rutaArchivos, baseName + ".pdf");
                        if (!File.Exists(elegido))
                        {
                            // como último recurso, que el controlador de diploma copie el original
                            // pero aquí evitamos dependencia; si no existe, omitimos este id
                            _logger.LogWarning("No existe documento para solicitud {id}", id);
                            continue;
                        }
                    }
                    var destino = Path.Combine(tempDir, baseName + ".pdf");
                    File.Copy(elegido, destino, overwrite: true);
                }

                // Empaquetar ZIP
                string zipPath = Path.Combine(Path.GetTempPath(), "cmp_lote_" + Guid.NewGuid().ToString("N") + ".zip");
                ZipFile.CreateFromDirectory(tempDir, zipPath);

                var zipBytes = await File.ReadAllBytesAsync(zipPath);
                string zipBase64 = Convert.ToBase64String(zipBytes);
                string nombreZip = Path.GetFileName(zipPath);

                // Enviar al firmador como lote (.zip)
                // Reutilizamos el método de FirmaDigitalService para enviar carga pendiente con coordenadas
                string coords = tipoDocumentoFirmado switch
                {
                    1 => _config["FirmaDigital:CoordenadasFirmaCRSecretario"],
                    2 => _config["FirmaDigital:CoordenadasFirmaCRDecano"],
                    3 => _config["FirmaDigital:CoordenadasFirmaSGSecretario"],
                    4 => _config["FirmaDigital:CoordenadasFirmaDecano"],
                    _ => _config["FirmaDigital:CoordenadasFirmador"]
                } ?? _config["FirmaDigital:CoordenadasFirmador"];

                // llamamos al método interno via reflexión mínima: duplicamos payload localmente
                var method = _singleService.GetType().GetMethod("ObtenerTokenAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                var tokenTaskObj = method.Invoke(_singleService, null);
                var tokenTask = tokenTaskObj as Task<TokenResponse>;
                var tokenResp = tokenTask != null ? await tokenTask : null;
                if (tokenResp == null)
                    return new UploadResponse { codigoFirma = -1, descripcion = "No se pudo generar el token de seguridad" };

                var urlFirmador = _config["FirmaDigital:UrlFirmador"];
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlFirmador}/api/firma-carga-pendiente");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResp.access_token);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var payload = new
                {
                    archivo = zipBase64,
                    nombre = nombreZip, // terminar en .zip es requisito
                    parametros = new
                    {
                        posicionFirma = "CO",
                        ubicacionPagina = "PP",
                        coordenadas = coords,
                        estiloFirma = "ID",
                        invisible = "1",
                        aplicarImagen = "0",
                        imagen = "",
                        rutaImagen = _config["FirmaDigital:HostImagenesFirmaPeru"],
                        altoImagen = 60,
                        anchoImagen = 60,
                        aplicarTsa = 1
                    }
                };

                request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al enviar lote para firma: {err}", err);
                    return new UploadResponse { codigoFirma = -1, descripcion = "Error al iniciar firma en lote" };
                }
                var json = await response.Content.ReadAsStringAsync();
                var up = JsonSerializer.Deserialize<UploadResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return up ?? new UploadResponse { codigoFirma = -1, descripcion = "Respuesta inválida del servicio" };
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        public async Task<DownloadResponse> SubirLoteFirmadoAsync(int codigoFirma)
        {
            // Reutilizamos descarga del servicio single
            var method = _singleService.GetType().GetMethod("ObtenerTokenAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var tokenTask = method.Invoke(_singleService, null) as Task<TokenResponse>;
            var token = tokenTask != null ? await tokenTask : null;
            if (token == null)
                return new DownloadResponse { estado = -1, descripcion = "No se pudo generar el token de seguridad" };

            var downloadMethod = _singleService.GetType().GetMethod("DescargarDocumentoFirmadoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var task = downloadMethod.Invoke(_singleService, new object[] { token.access_token, codigoFirma }) as Task<DownloadResponse?>;
            var resp = task != null ? await task : null;
            return resp ?? new DownloadResponse { estado = -1, descripcion = "No se pudo descargar el archivo firmado" };
        }
    }
}


