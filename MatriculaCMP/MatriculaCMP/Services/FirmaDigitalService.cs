using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MatriculaCMP.Shared.FirmaDigitalDTO;

namespace MatriculaCMP.Services
{
    public class FirmaDigitalService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FirmaDigitalService> _logger;
        public FirmaDigitalService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<FirmaDigitalService> logger)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<UploadResponse> FirmarDocumentoAsync(FirmaRequest request)
        {
            try
            {
                _logger.LogInformation($"Iniciando proceso de firma para documento {request.IdExpedienteDocumento}");
                var token = await ObtenerTokenAsync();
                if (token == null)
                {
                    _logger.LogError("No se pudo obtener token de autenticación");
                    return new UploadResponse { codigoFirma = -1, descripcion = "No se pudo generar el token de seguridad" };
                }

                // Verificar que el token tenga los scopes necesarios
                if (string.IsNullOrEmpty(token.scope))
                {
                    _logger.LogWarning("Token no contiene scopes definidos");
                }
                else
                {
                    _logger.LogInformation($"Scopes del token: {token.scope}");
                }
                // Obtener el documento a firmar: exactamente el preparado en wwwroot/firmas_digitales/documento_{SolicitudId}.pdf
                string rutaDocumento = Path.Combine(_config["FirmaDigital:RutaArchivos"], $"documento_{request.IdExpedienteDocumento}.pdf");
                // Abrir en modo compartido lectura para evitar conflictos de bloqueo
                byte[] pdfBytes;
                using (var fs = new FileStream(rutaDocumento, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using var ms = new MemoryStream();
                    await fs.CopyToAsync(ms);
                    pdfBytes = ms.ToArray();
                }
                string pdfBase64 = Convert.ToBase64String(pdfBytes);
                string nombreArchivo = Path.GetFileName(rutaDocumento);

                bool indAval = true; //request.IdAvalMedico > 0; // Simplificación de la lógica de aval

                // Seleccionar coordenadas según el actor/etapa (TipoDocumentoFirmado como selector temporal)
                string coordenadas = request.TipoDocumentoFirmado switch
                {
                    1 => _config["FirmaDigital:CoordenadasFirmaCRSecretario"],
                    2 => _config["FirmaDigital:CoordenadasFirmaCRDecano"],
                    3 => _config["FirmaDigital:CoordenadasFirmaSGSecretario"],
                    4 => _config["FirmaDigital:CoordenadasFirmaDecano"],
                    _ => _config["FirmaDigital:CoordenadasFirmador"]
                } ?? _config["FirmaDigital:CoordenadasFirmador"];

                var responseEnvio = await EnviarFirmaCargaPendienteAsync(token.access_token, pdfBase64, nombreArchivo, indAval, coordenadas);

                return responseEnvio ?? new UploadResponse { codigoFirma = -1, descripcion = "No se pudo cargar el documento al servicio" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en FirmarDocumentoAsync: {ex.Message}");
                return new UploadResponse { codigoFirma = -1, descripcion = $"Error: {ex.Message}" };
            }
        }

        public async Task<DownloadResponse> SubirDocumentoFirmadoAsync(UploadRequest request)
        {
            try
            {
                var token = await ObtenerTokenAsync();
                if (token == null)
                {
                    return new DownloadResponse { estado = -1, descripcion = "No se pudo generar el token de seguridad" };
                }

                var responseDescarga = await DescargarDocumentoFirmadoAsync(token.access_token, request.CodigoFirma);

                if (responseDescarga != null && responseDescarga.estado > 0 && !string.IsNullOrEmpty(responseDescarga.archivoFirmado))
                {
                    // Guardar el archivo firmado con un nombre determinístico por solicitud
                    string nombreArchivo = $"documento_{request.IdExpedienteDocumento}_firmado.pdf";
                    string rutaCarpeta = _config["FirmaDigital:RutaArchivos"];
                    Directory.CreateDirectory(rutaCarpeta);
                    string rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);

                    byte[] archivoBytes = Convert.FromBase64String(responseDescarga.archivoFirmado);
                    await File.WriteAllBytesAsync(rutaArchivo, archivoBytes);

                    // Reemplazar el PDF de trabajo para próximas firmas: documento_{id}.pdf = firmado
                    try
                    {
                        var rutaTrabajo = Path.Combine(rutaCarpeta, $"documento_{request.IdExpedienteDocumento}.pdf");
                        File.Copy(rutaArchivo, rutaTrabajo, overwrite: true);
                    }
                    catch { }

                    // Nota: actualización de BD y avance de estado se realizará en el controlador que conoce Solicitud/Diploma

                    return responseDescarga;
                }

                return responseDescarga ?? new DownloadResponse { estado = -1, descripcion = "No se pudo descargar el archivo firmado" };
            }
            catch (Exception ex)
            {
                return new DownloadResponse { estado = -1, descripcion = $"Error: {ex.Message}" };
            }
        }

        private async Task<TokenResponse> ObtenerTokenAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_config["FirmaDigital:UrlFirmador"]}/oauth/token");

                var authUsuario = _config["FirmaDigital:AuthUsuarioFirmador"];
                var authPassword = _config["FirmaDigital:AuthPasswordFirmador"];

                if (string.IsNullOrEmpty(authUsuario) || string.IsNullOrEmpty(authPassword))
                {
                    _logger.LogError("Credenciales de autenticación no configuradas");
                    return null;
                }

                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authUsuario}:{authPassword}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                request.Headers.Add("Cookie", "JSESSIONID=EF_HdMytotNeYwUm-Tlk_arUHwgtGhZWEwl0_6RW.srv-firdigital");

                var formData = new List<KeyValuePair<string, string>>
        {
            new("username", _config["FirmaDigital:UsuarioFirmador"] ?? string.Empty),
            new("password", _config["FirmaDigital:PasswordFirmador"] ?? string.Empty),
            new("grant_type", "password")
        };

                request.Content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error al obtener token. Status: {response.StatusCode}. Respuesta: {errorContent}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TokenResponse>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ObtenerTokenAsync: {ex.Message}");
                return null;
            }
        }

        //private async Task<DownloadResponse> DescargarDocumentoFirmadoAsync(string token, int codigo)
        //{
        //    try
        //    {
        //        var request = new HttpRequestMessage(HttpMethod.Post,
        //            $"{_config["FirmaDigital:UrlFirmador"]}/api/firma-descarga-firmado");

        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //        var payload = new { codigo };
        //        var jsonContent = JsonSerializer.Serialize(payload,
        //            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        //        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        //        var response = await _httpClient.SendAsync(request);
        //        response.EnsureSuccessStatusCode();

        //        var jsonResponse = await response.Content.ReadAsStringAsync();
        //        return JsonSerializer.Deserialize<DownloadResponse>(jsonResponse,
        //            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


        private async Task<DownloadResponse?> DescargarDocumentoFirmadoAsync(string token, int codigo)
        {
            try
            {
                var url = $"{_config["FirmaDigital:UrlFirmador"]}/api/firma-descarga-firmado";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var payload = new { codigo };

                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    // Puedes registrar el error si quieres más detalle
                    var errorText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {response.StatusCode} - {errorText}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<DownloadResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                // Para depuración: Console.WriteLine(ex.Message);
                return null;
            }
        }


        private async Task<UploadResponse> EnviarFirmaCargaPendienteAsync(string token, string archivoBase64,
            string nombreArchivo, bool indAval, string coordenadas)
        {
            try
            {
                var urlFirmador = _config["FirmaDigital:UrlFirmador"];
                if (string.IsNullOrEmpty(urlFirmador))
                {
                    _logger.LogError("URL del firmador no configurada");
                    return new UploadResponse
                    {
                        codigoFirma = -1,
                        descripcion = "Configuración del servicio no válida"
                    };
                }

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{urlFirmador}/api/firma-carga-pendiente");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //coordenadas = indAval ? _config["FirmaDigital:CoordenadasFirmador"] : _config["FirmaDigital:CoordenadasFirmadorCentro"],
                var payload = new
                {
                    archivo = archivoBase64,
                    nombre = nombreArchivo,
                    parametros = new
                    {
                        posicionFirma = "CO",
                        ubicacionPagina = "PP",
                        coordenadas = coordenadas,
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

                var jsonContent = JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                request.Content = new StringContent(
                    jsonContent,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error al enviar documento para firma. Status: {response.StatusCode}. Respuesta: {errorContent}");

                    return new UploadResponse
                    {
                        codigoFirma = -1,
                        descripcion = $"Error en el servicio: {response.StatusCode}"
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UploadResponse>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new UploadResponse
                {
                    codigoFirma = -1,
                    descripcion = "Respuesta inválida del servicio de firma"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en EnviarFirmaCargaPendienteAsync: {ex.Message}");
                return new UploadResponse
                {
                    codigoFirma = -1,
                    descripcion = $"Error: {ex.Message}"
                };
            }
        }

        private string ObtenerRutaDocumento(int idExpedienteDocumento, int tipoDocumentoFirmado)
        {
            // Implementa la lógica para obtener la ruta del documento según tu aplicación
            // Esto es un ejemplo simplificado
            string rutaBase = _config["FirmaDigital:RutaArchivos"];
            return Path.Combine(rutaBase, $"documento_{idExpedienteDocumento}.pdf");
        }
    }
}
