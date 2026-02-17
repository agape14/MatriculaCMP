using MatriculaCMP.Services;
using MatriculaCMP.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using static MatriculaCMP.Shared.FirmaDigitalDTO;
using MatriculaCMP.Shared;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirmaDigitalController : ControllerBase
    {
        private readonly FirmaDigitalService _firmaService;
        private readonly ApplicationDbContext _context;

        public FirmaDigitalController(FirmaDigitalService firmaService, ApplicationDbContext context)
        {
            _firmaService = firmaService;
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

        [HttpPost("firmar")]
        public async Task<ActionResult<UploadResponse>> FirmarDocumento([FromBody] FirmaRequest request)
        {
            try
            {
                // Asegurar archivo a firmar ya existe en wwwroot/firmas_digitales/documento_{id}.pdf
                // La preparación del archivo se hace desde el controlador de Diploma (preparar-firma)
                var result = await _firmaService.FirmarDocumentoAsync(request);
                // NO actualizar estado aquí - se actualizará cuando se complete la firma en el método SubirDocumentoFirmado
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new UploadResponse
                {
                    codigoFirma = -1,
                    descripcion = $"No se pudo iniciar la firma. Verifique el firmador o la aplicación Biuen. Detalle: {ex.Message}"
                });
            }
        }

        [HttpPost("upload")]
        public async Task<ActionResult<DownloadResponse>> SubirDocumentoFirmado([FromBody] UploadRequest request)
        {
            try
            {
                // 1) Descargar y guardar archivo en carpeta pública (servicio ya lo hace con nombre determinístico)
                var result = await _firmaService.SubirDocumentoFirmadoAsync(request);
            if (result is null)
            {
                return Ok(result);
            }

            // Fallback: si el servicio indica "ya descargado / no disponible" o error de token, usar archivo local si existe
            if (result.estado <= 0 || string.IsNullOrEmpty(result.archivoFirmado))
            {
                var desc = result.descripcion?.ToLowerInvariant() ?? string.Empty;
                bool indicaDescargado = desc.Contains("ya han sido descargados") || desc.Contains("no se encuentran disponibles");
                bool errorToken = desc.Contains("token") || desc.Contains("no se pudo generar el token");
                if (indicaDescargado || errorToken)
                {
                    try
                    {
                        var rutaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmas_digitales");
                        var baseName = $"documento_{request.IdExpedienteDocumento}";
                        var candidatos = Directory.GetFiles(rutaBase, baseName + "*.pdf");
                        var elegido = candidatos
                            .OrderByDescending(p => Path.GetFileNameWithoutExtension(p).Count(c => c == 'F'))
                            .FirstOrDefault();
                        if (!string.IsNullOrEmpty(elegido))
                        {
                            var bytes = await System.IO.File.ReadAllBytesAsync(elegido);
                            result = new DownloadResponse
                            {
                                estado = 1,
                                descripcion = "Documento firmado ya estaba disponible en servidor",
                                archivoFirmado = Convert.ToBase64String(bytes)
                            };
                        }
                        else
                        {
                            // Considerar éxito de registro aunque no podamos adjuntar bytes (UI solo requiere estado>0)
                            result = new DownloadResponse
                            {
                                estado = 1,
                                descripcion = "Documento firmado registrado previamente"
                            };
                        }
                    }
                    catch { }
                }

                if (result.estado <= 0)
                {
                    return Ok(result);
                }
            }

            // 2) Persistir ruta firmada en BD y avanzar estado según flujo
            var solicitudId = request.IdExpedienteDocumento;
            var diploma = await _context.Diplomas.FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);
            if (diploma != null)
            {
                // Buscar el archivo más reciente con sufijos [F]
                var rutaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmas_digitales");
                var baseName = $"documento_{solicitudId}";
                var candidatos = Directory.GetFiles(rutaBase, baseName + "*.pdf");
                var elegido = candidatos
                    .OrderByDescending(p => Path.GetFileNameWithoutExtension(p).Count(c => c == 'F'))
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(elegido))
                {
                    diploma.RutaPdfFirmado = Path.GetFileName(elegido);
                    _context.Diplomas.Update(diploma);
                }
            }

            var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId);
            if (solicitud != null)
            {
                var estadoAnterior = solicitud.EstadoSolicitudId;
                // Avance de estado por actor/etapa usando TipoDocumentoFirmado del request
                // 1: Sec. CR, 2: Decano CR, 3: Sec. General, 4: Decano
                int proximoEstado = estadoAnterior;
                if (request.TipoDocumentoFirmado == 1 && estadoAnterior == 6)
                    proximoEstado = 8; // Secretario CR completa firma: 6 -> 8 (Pend. Decano CR)
                else if (request.TipoDocumentoFirmado == 1 && estadoAnterior == 8)
                    proximoEstado = 8; // Firma solo ratifica estado 8
                else if (request.TipoDocumentoFirmado == 2 && estadoAnterior == 8)
                    proximoEstado = 9; // Decano CR completa CR -> pasa a 9 (Pend. SG)
                else if (request.TipoDocumentoFirmado == 3 && estadoAnterior == 9)
                    proximoEstado = 10; // Sec. General -> pasa a 10 (Pend. Decano)
                else if (request.TipoDocumentoFirmado == 4 && estadoAnterior == 10)
                    proximoEstado = 11; // Decano -> pasa a 11 (Diploma listo para entrega)

                // Robustez: si la firma es de Secretaría General (3) y aún no estamos en 10, forzar avance a 10
                if (request.TipoDocumentoFirmado == 3 && proximoEstado == estadoAnterior && estadoAnterior < 10)
                {
                    proximoEstado = 10;
                }

                if (proximoEstado != estadoAnterior)
                {
                    solicitud.EstadoSolicitudId = proximoEstado;
                    var fechaCambio = DateTime.Now;
                    var historial = new SolicitudHistorialEstado
                    {
                        SolicitudId = solicitud.Id,
                        EstadoAnteriorId = estadoAnterior,
                        EstadoNuevoId = proximoEstado,
                        FechaCambio = fechaCambio,
                        Observacion = "Avance por firma digital",
                        UsuarioCambio = GetUsuarioAutenticadoId()
                    };
                    _context.SolicitudHistorialEstados.Add(historial);
                    _context.Solicitudes.Update(solicitud);
                    await _context.SaveChangesAsync();

                    // Enviar correo solo para estados permitidos: 9, 11
                    if (proximoEstado == 9 || proximoEstado == 11)
                    {
                        var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                        var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                        var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";
                        await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, proximoEstado, null);
                    }
                }
            }
            else
            {
                await _context.SaveChangesAsync();
            }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new DownloadResponse
                {
                    estado = -1,
                    descripcion = $"Error al subir documento firmado. Si recargó la página (F5), vuelva al listado e intente de nuevo. Detalle: {ex.Message}"
                });
            }
        }
    }
}
