using MatriculaCMP.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using MatriculaCMP.Server.Data;
using Microsoft.EntityFrameworkCore;
using MatriculaCMP.Shared;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/firmadigital-lote")]
    public class FirmaDigitalLoteController : ControllerBase
    {
        private readonly FirmaDigitalLoteService _loteService;
        private readonly ApplicationDbContext _context;

        public FirmaDigitalLoteController(FirmaDigitalLoteService loteService, ApplicationDbContext context)
        {
            _loteService = loteService;
            _context = context;
        }

        private string GetUsuarioAutenticadoId()
        {
            var user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId)) return userId;
            }
            return "Sistema";
        }

        public class FirmarLoteDto
        {
            public List<int> Ids { get; set; } = new();
            public int TipoDocumentoFirmado { get; set; }
        }

        [HttpPost("firmar")]
        public async Task<IActionResult> Firmar([FromBody] FirmarLoteDto dto)
        {
            try
            {
                var r = await _loteService.FirmarLoteAsync(dto.Ids, dto.TipoDocumentoFirmado);
                return Ok(r ?? new FirmaDigitalDTO.UploadResponse { codigoFirma = -1, descripcion = "Error al iniciar firma en lote" });
            }
            catch (Exception ex)
            {
                return Ok(new FirmaDigitalDTO.UploadResponse
                {
                    codigoFirma = -1,
                    descripcion = $"No se pudo iniciar la firma en lote. Verifique el firmador o la aplicación Biuen. Detalle: {ex.Message}"
                });
            }
        }

        public class UploadLoteDto
        {
            public int CodigoFirma { get; set; }
            public int TipoDocumentoFirmado { get; set; }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Subir([FromBody] UploadLoteDto dto)
        {
            try
            {
                var resp = await _loteService.SubirLoteFirmadoAsync(dto.CodigoFirma);
            if (resp == null || resp.estado <= 0 || string.IsNullOrEmpty(resp.archivoFirmado))
                return Ok(resp);

            // Si es ZIP, procesar múltiple
            byte[] bytes = Convert.FromBase64String(resp.archivoFirmado);
            string tempZip = Path.Combine(Path.GetTempPath(), "cmp_lote_firmado_" + Guid.NewGuid().ToString("N") + ".zip");
            await System.IO.File.WriteAllBytesAsync(tempZip, bytes);

            string extractDir = Path.Combine(Path.GetTempPath(), "cmp_lote_extract_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(extractDir);
            try
            {
                ZipFile.ExtractToDirectory(tempZip, extractDir);
            }
            catch
            {
                // No es ZIP: tratar como single documento; dejarlo en memoria
                extractDir = string.Empty;
            }

            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmas_digitales");
            Directory.CreateDirectory(rutaCarpeta);

            if (!string.IsNullOrEmpty(extractDir) && Directory.Exists(extractDir))
            {
                foreach (var file in Directory.GetFiles(extractDir, "*.pdf"))
                {
                    var name = Path.GetFileName(file); // se espera documento_{Id}.pdf
                    var sinExt = Path.GetFileNameWithoutExtension(name);
                    // extraer Id
                    var parts = sinExt.Split('_');
                    if (parts.Length < 2 || !int.TryParse(parts[1], out var solicitudId))
                        continue;

                    var baseName = $"documento_{solicitudId}";
                    var existentes = Directory.GetFiles(rutaCarpeta, baseName + "*.pdf");
                    int firmasPrevias = existentes
                        .Select(p => Path.GetFileNameWithoutExtension(p))
                        .Select(n => n.Count(c => c == 'F'))
                        .DefaultIfEmpty(0)
                        .Max();
                    string nuevoNombre = firmasPrevias <= 0
                        ? $"{baseName}[F].pdf"
                        : $"{baseName}{string.Concat(Enumerable.Repeat("[F]", firmasPrevias + 1))}.pdf";
                    string nuevoPath = Path.Combine(rutaCarpeta, nuevoNombre);
                    System.IO.File.Copy(file, nuevoPath, overwrite: true);
                    foreach (var p in existentes)
                    {
                        try { if (!string.Equals(p, nuevoPath, StringComparison.OrdinalIgnoreCase)) System.IO.File.Delete(p); } catch { }
                    }

                    var diploma = await _context.Diplomas.FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);
                    if (diploma != null)
                    {
                        diploma.RutaPdfFirmado = Path.GetFileName(nuevoPath);
                        _context.Diplomas.Update(diploma);
                    }
                    var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId);
                    if (solicitud != null)
                    {
                        int estadoAnterior = solicitud.EstadoSolicitudId;
                        int proximo = estadoAnterior;
                        // 1: Secretario CR firma (lote): 6 -> 8 cuando se completa la firma
                        if (dto.TipoDocumentoFirmado == 1 && estadoAnterior == 6) proximo = 8;
                        // Si ya está en 8, mantenerlo
                        else if (dto.TipoDocumentoFirmado == 1 && estadoAnterior == 8) proximo = 8;
                        // 2: Decano CR firma => 8 (Pend. Sec CR) -> 9 (Pend. Decano CR) -> 10 para SG al completar Decano CR
                        else if (dto.TipoDocumentoFirmado == 2 && estadoAnterior == 8) proximo = 9;
                        // 3: Secretario General firma => 9 -> 10
                        else if (dto.TipoDocumentoFirmado == 3 && estadoAnterior == 9) proximo = 10;
                        // Robustez: si es SG (3) y aún no se alcanzó 10, forzar a 10
                        else if (dto.TipoDocumentoFirmado == 3 && estadoAnterior < 10) proximo = 10;
                        // 4: Decano firma => 10 -> 11
                        else if (dto.TipoDocumentoFirmado == 4 && estadoAnterior == 10) proximo = 11;
                        if (proximo != estadoAnterior)
                        {
                            solicitud.EstadoSolicitudId = proximo;
                            _context.Solicitudes.Update(solicitud);
                            _context.SolicitudHistorialEstados.Add(new SolicitudHistorialEstado
                            {
                                SolicitudId = solicitud.Id,
                                EstadoAnteriorId = estadoAnterior,
                                EstadoNuevoId = proximo,
                                FechaCambio = DateTime.Now,
                                Observacion = "Avance por firma digital (lote)",
                                UsuarioCambio = GetUsuarioAutenticadoId()
                            });
                            
                            // Enviar correo solo para estados permitidos: 9, 11
                            if (proximo == 9 || proximo == 11)
                            {
                                var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                                var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                                var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";
                                await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, proximo, null);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                // Single PDF: no viene comprimido; no soportado para lote en este método
            }

                return Ok(resp);
            }
            catch (Exception ex)
            {
                return Ok(new FirmaDigitalDTO.DownloadResponse
                {
                    estado = -1,
                    descripcion = $"Error al subir lote firmado. Si recargó la página (F5), vuelva al listado e intente de nuevo. Detalle: {ex.Message}"
                });
            }
        }
    }
}


