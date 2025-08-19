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

        [HttpPost("firmar")]
        public async Task<ActionResult<UploadResponse>> FirmarDocumento([FromBody] FirmaRequest request)
        {
            // Asegurar archivo a firmar ya existe en wwwroot/firmas_digitales/documento_{id}.pdf
            // La preparación del archivo se hace desde el controlador de Diploma (preparar-firma)

            // Si la solicitud está en 6 (Aprobado Of. Matrícula) aún no se envió a firma CR: pasar a 8 (Pend. Firma Sec. CR)
            var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == request.IdExpedienteDocumento);
            if (solicitud != null && solicitud.EstadoSolicitudId == 6)
            {
                int estadoAnterior = solicitud.EstadoSolicitudId;
                solicitud.EstadoSolicitudId = 8;
                _context.Solicitudes.Update(solicitud);
                _context.SolicitudHistorialEstados.Add(new SolicitudHistorialEstado
                {
                    SolicitudId = solicitud.Id,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = 8,
                    FechaCambio = DateTime.Now,
                    Observacion = "Enviado a firma Secretario CR",
                    UsuarioCambio = "Sistema"
                });
                await _context.SaveChangesAsync();
            }

            var result = await _firmaService.FirmarDocumentoAsync(request);
            return Ok(result);
        }

        [HttpPost("upload")]
        public async Task<ActionResult<DownloadResponse>> SubirDocumentoFirmado([FromBody] UploadRequest request)
        {
            // 1) Descargar y guardar archivo en carpeta pública (servicio ya lo hace con nombre determinístico)
            var result = await _firmaService.SubirDocumentoFirmadoAsync(request);
            if (result is null || result.estado <= 0)
            {
                return Ok(result);
            }

            // 2) Persistir ruta firmada en BD y avanzar estado según flujo
            var solicitudId = request.IdExpedienteDocumento;
            var diploma = await _context.Diplomas.FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);
            if (diploma != null)
            {
                diploma.RutaPdfFirmado = $"documento_{solicitudId}_firmado.pdf";
                _context.Diplomas.Update(diploma);
            }

            var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId);
            if (solicitud != null)
            {
                var estadoAnterior = solicitud.EstadoSolicitudId;
                // Avance de estado por actor/etapa usando TipoDocumentoFirmado del request
                // 1: Sec. CR, 2: Decano CR, 3: Sec. General, 4: Decano
                int proximoEstado = estadoAnterior;
                if (request.TipoDocumentoFirmado == 1 && estadoAnterior == 8)
                    proximoEstado = 8; // Firma solo ratifica estado 8; avance real se hace al iniciar (6->8)
                else if (request.TipoDocumentoFirmado == 2 && estadoAnterior == 8)
                    proximoEstado = 9; // Decano CR completa CR -> pasa a 9 (Pend. SG)
                else if (request.TipoDocumentoFirmado == 3 && estadoAnterior == 9)
                    proximoEstado = 10; // Sec. General -> pasa a 10 (Pend. Decano)
                else if (request.TipoDocumentoFirmado == 4 && estadoAnterior == 10)
                    proximoEstado = 11; // Decano -> pasa a 11 (Diploma listo para entrega)

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
                        UsuarioCambio = "Sistema"
                    };
                    _context.SolicitudHistorialEstados.Add(historial);
                    _context.Solicitudes.Update(solicitud);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }
}
