using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OficinaMatriculaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public OficinaMatriculaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método helper para obtener el ID del usuario autenticado
        private string GetUsuarioAutenticadoId()
        {
            var user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
            }
            return "Sistema"; // Fallback si no se puede obtener el ID del usuario
        }

        [HttpGet("solicitudes-pendientes")]
        public async Task<ActionResult<IEnumerable<SolicitudSeguimientoDto>>> GetSolicitudesPendientes()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EstadoSolicitudId == 2 || s.EstadoSolicitudId == 6 || s.EstadoSolicitudId == 11) // + Estado 11: Proceso finalizado - Entregado (firmado por Decano)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            // Obtener los diplomas existentes para estas solicitudes
            var diplomasExistentes = await _context.Diplomas
                .Where(d => solicitudes.Select(s => s.Id).Contains(d.SolicitudId))
                .Select(d => d.SolicitudId)
                .ToListAsync();

            // Debug: Log the raw data from database
            foreach (var s in solicitudes.Where(s => s.EstadoSolicitudId == 6))
            {
                Console.WriteLine($"DB - Solicitud {s.Id}: Persona.NumeroColegiatura = '{s.Persona?.NumeroColegiatura}'");
            }

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud.Nombre,
                EstadoId = s.EstadoSolicitud.Id,
                EstadoColor = s.EstadoSolicitud.Color,
                AreaNombre = s.Area != null ? s.Area.Nombre : "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones.FirstOrDefault()?.Universidad?.Nombre ?? "-",
                NumeroColegiatura = s.Persona?.NumeroColegiatura,
                TieneDiploma = diplomasExistentes.Contains(s.Id)
            }).ToList();

            // Debug: Log the NumeroColegiatura values
            foreach (var item in resultado.Where(r => r.EstadoId == 6))
            {
                Console.WriteLine($"Solicitud {item.Id}: NumeroColegiatura = '{item.NumeroColegiatura}'");
            }

            return Ok(resultado);
        }

        [HttpGet("solicitud-detalle/{id}")]
        public async Task<ActionResult<Solicitud>> GetDetalleSolicitud(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.GrupoSanguineo)
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Documento)
                .Include(s => s.EstadoSolicitud)
                .Include(s => s.HistorialEstados)
                    .ThenInclude(h => h.EstadoAnterior)
                .Include(s => s.HistorialEstados)
                    .ThenInclude(h => h.EstadoNuevo)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound("Solicitud no encontrada");

            return Ok(solicitud);
        }

        [HttpPost("aprobar-solicitud")]
        public async Task<IActionResult> AprobarSolicitud([FromBody] SolicitudCambioEstadoDto dto)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

                if (solicitud is null)
                    return NotFound("Solicitud no encontrada");

                // Verificar que la solicitud esté en estado aprobado por CR (estado 2)
                if (solicitud.EstadoSolicitudId != 2)
                    return BadRequest("La solicitud debe estar aprobada por el Consejo Regional para continuar");

                var estadoAnterior = solicitud.EstadoSolicitudId;

                // Cambiar estado a aprobado por Oficina de Matrícula (estado 6)
                solicitud.EstadoSolicitudId = 6;
                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Obtener el ID del usuario autenticado
                var usuarioId = GetUsuarioAutenticadoId();

                // Guardar en historial
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = dto.SolicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = 6,
                    FechaCambio = fechaCambio,
                    Observacion = dto.Observacion ?? "Aprobado por Oficina de Matrícula",
                    UsuarioCambio = usuarioId // Usar el ID del usuario autenticado
                };

                _context.SolicitudHistorialEstados.Add(historial);
                await _context.SaveChangesAsync();

                // Enviar email de notificación
                var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";

                await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, "Aprobado por Oficina de Matrícula", dto.Observacion);

                return Ok(new { message = "Solicitud aprobada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error al aprobar la solicitud: {ex.Message}");
            }
        }

        [HttpPost("rechazar-solicitud")]
        public async Task<IActionResult> RechazarSolicitud([FromBody] SolicitudCambioEstadoDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Observacion))
                    return BadRequest("La observación es requerida para rechazar una solicitud");

                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

                if (solicitud is null)
                    return NotFound("Solicitud no encontrada");

                // Verificar que la solicitud esté en estado aprobado por CR (estado 2)
                if (solicitud.EstadoSolicitudId != 2)
                    return BadRequest("La solicitud debe estar aprobada por el Consejo Regional para continuar");

                var estadoAnterior = solicitud.EstadoSolicitudId;

                // Cambiar estado a rechazado por Oficina de Matrícula (estado 7)
                solicitud.EstadoSolicitudId = 7;
                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Obtener el ID del usuario autenticado
                var usuarioId = GetUsuarioAutenticadoId();

                // Guardar en historial
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = dto.SolicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = 7,
                    FechaCambio = fechaCambio,
                    Observacion = dto.Observacion,
                    UsuarioCambio = usuarioId // Usar el ID del usuario autenticado
                };

                _context.SolicitudHistorialEstados.Add(historial);
                await _context.SaveChangesAsync();

                // Enviar email de notificación
                var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";

                await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, "Rechazado por Oficina de Matrícula", dto.Observacion);

                return Ok(new { message = "Solicitud rechazada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error al rechazar la solicitud: {ex.Message}");
            }
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<object>> GetEstadisticas()
        {
            try
            {
                var totalSolicitudes = await _context.Solicitudes.CountAsync();
                var solicitudesPendientes = await _context.Solicitudes.CountAsync(s => s.EstadoSolicitudId == 2);
                var solicitudesAprobadas = await _context.Solicitudes.CountAsync(s => s.EstadoSolicitudId == 6);
                var solicitudesRechazadas = await _context.Solicitudes.CountAsync(s => s.EstadoSolicitudId == 7);

                return Ok(new
                {
                    TotalSolicitudes = totalSolicitudes,
                    Pendientes = solicitudesPendientes,
                    Aprobadas = solicitudesAprobadas,
                    Rechazadas = solicitudesRechazadas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener estadísticas: {ex.Message}");
            }
        }
    }
} 