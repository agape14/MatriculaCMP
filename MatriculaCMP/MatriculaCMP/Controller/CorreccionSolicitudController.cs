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
    public class CorreccionSolicitudController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public CorreccionSolicitudController(ApplicationDbContext context)
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

        [HttpPost("reenviar-solicitud/{solicitudId}")]
        public async Task<IActionResult> ReenviarSolicitud(int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                // Verificar que la solicitud esté en estado rechazado (3, 5 o 7)
                if (solicitud.EstadoSolicitudId != 3 && solicitud.EstadoSolicitudId != 5 && solicitud.EstadoSolicitudId != 7)
                    return BadRequest("La solicitud debe estar en estado rechazado para ser reenviada");

                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Determinar el estado al que debe regresar usando el historial
                int nuevoEstadoId;
                string observacion;
                
                // Obtener el último registro del historial para determinar el estado anterior
                var ultimoHistorial = await _context.SolicitudHistorialEstados
                    .Where(h => h.SolicitudId == solicitudId)
                    .OrderByDescending(h => h.FechaCambio)
                    .FirstOrDefaultAsync();
                
                if (ultimoHistorial != null && ultimoHistorial.EstadoAnteriorId.HasValue)
                {
                    // Usar el EstadoAnteriorId del historial para regresar al estado correcto
                    nuevoEstadoId = ultimoHistorial.EstadoAnteriorId.Value;
                    observacion = $"Solicitud corregida y reenviada. Regresando al estado anterior (ID: {nuevoEstadoId})";
                }
                else
                {
                    // Fallback: determinar el estado al que debe regresar según el último estado
                    if (solicitud.EstadoSolicitudId == 7) // Rechazado por Oficina de Matrícula
                    {
                        nuevoEstadoId = 2; // Volver a Aprobado por Consejo Regional
                        observacion = "Solicitud corregida y reenviada desde Oficina de Matrícula";
                    }
                    else if (solicitud.EstadoSolicitudId == 5) // Rechazado por Secretaría General
                    {
                        nuevoEstadoId = 4; // Volver a Aprobado por Secretaría General
                        observacion = "Solicitud corregida y reenviada desde Secretaría General";
                    }
                    else // Estado 3 - Rechazado por Consejo Regional
                    {
                        nuevoEstadoId = 1; // Volver a Registrado
                        observacion = "Solicitud corregida y reenviada desde Consejo Regional";
                    }
                }

                var estadoAnterior = solicitud.EstadoSolicitudId;
                solicitud.EstadoSolicitudId = nuevoEstadoId;

                // Obtener el ID del usuario autenticado
                var usuarioId = GetUsuarioAutenticadoId();

                // Guardar en historial
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = solicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = nuevoEstadoId,
                    FechaCambio = fechaCambio,
                    Observacion = observacion,
                    UsuarioCambio = usuarioId // Usar el ID del usuario autenticado
                };

                _context.SolicitudHistorialEstados.Add(historial);
                await _context.SaveChangesAsync();

                // No enviar correo aquí - el correo se enviará automáticamente cuando el estado cambie a uno de los permitidos (1, 6, 7, 9, 11, 13)
                // Si el nuevoEstadoId es uno de los permitidos, se enviará el correo automáticamente desde PersonasEducacionController

                return Ok(new { 
                    success = true, 
                    message = "Solicitud reenviada exitosamente",
                    nuevoEstado = nuevoEstadoId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error al reenviar la solicitud: {ex.Message}" 
                });
            }
        }

        [HttpGet("solicitudes-corregibles")]
        public async Task<ActionResult<IEnumerable<SolicitudSeguimientoDto>>> GetSolicitudesCorregibles()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EstadoSolicitudId == 3 || s.EstadoSolicitudId == 5 || s.EstadoSolicitudId == 7)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

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
                UniversidadNombre = s.Persona?.Educaciones.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("historial-correcciones/{solicitudId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistorialCorrecciones(int solicitudId)
        {
            var historial = await _context.SolicitudHistorialEstados
                .Include(h => h.EstadoAnterior)
                .Include(h => h.EstadoNuevo)
                .Where(h => h.SolicitudId == solicitudId)
                .OrderByDescending(h => h.FechaCambio)
                .Select(h => new
                {
                    Fecha = h.FechaCambio,
                    EstadoAnterior = h.EstadoAnterior != null ? h.EstadoAnterior.Nombre : "N/A",
                    EstadoNuevo = h.EstadoNuevo.Nombre,
                    Observacion = h.Observacion,
                    Usuario = h.UsuarioCambio
                })
                .ToListAsync();

            return Ok(historial);
        }
        
        [HttpGet("observaciones-solicitud/{solicitudId}")]
        public async Task<ActionResult<object>> GetObservacionesSolicitud(int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.EstadoSolicitud)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                // Obtener la observación más reciente del historial
                var ultimaObservacion = await _context.SolicitudHistorialEstados
                    .Where(h => h.SolicitudId == solicitudId && !string.IsNullOrEmpty(h.Observacion))
                    .OrderByDescending(h => h.FechaCambio)
                    .Select(h => h.Observacion)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    solicitudId = solicitud.Id,
                    estadoActual = solicitud.EstadoSolicitud?.Nombre ?? "Sin estado",
                    estadoId = solicitud.EstadoSolicitudId,
                    observaciones = ultimaObservacion ?? "No hay observaciones registradas",
                    fechaSolicitud = solicitud.FechaSolicitud
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener observaciones: {ex.Message}" });
            }
        }
    }
} 