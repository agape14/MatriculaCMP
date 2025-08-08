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
    public class PersonasEducacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public PersonasEducacionController(ApplicationDbContext context)
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

        [HttpGet("con-educacion")]
        public async Task<ActionResult<IEnumerable<PersonaConEducacionDto>>> GetPersonasConEducacion()
        {
            var personas = await _context.Personas
                .Include(p => p.Educaciones) // Incluye las educaciones relacionadas
                .Select(p => new PersonaConEducacionDto
                {
                    PersonaId = p.Id,
                    NombresCompletos = $"{p.Nombres} {p.ApellidoPaterno}",
                    Documento = p.NumeroDocumento,
                    Educaciones = p.Educaciones.Select(e => new EducacionDto
                    {
                        UniversidadId = e.UniversidadId,
                        FechaEmision = e.FechaEmisionTitulo
                    }).ToList()
                })
                .ToListAsync();

            return Ok(personas);
        }

        // GET: api/personaseducacion/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Persona>> GetPersona(int id)
        {
            try
            {
                var persona = await _context.Personas
                    .Include(p => p.Educaciones)
                    .Include(p => p.Usuarios)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (persona == null)
                    return NotFound(new { message = $"No se encontró la persona con ID {id}" });

                return Ok(persona);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpGet("mis-solicitudes/{personaId:int}")]
        public async Task<IActionResult> ObtenerSolicitudesUsuario(int personaId)
        {
            try
            {
                var solicitudes = await _context.Solicitudes
                    .Include(s => s.Persona)
                        .ThenInclude(p => p.Educaciones)
                            .ThenInclude(e => e.Universidad)
                    .Include(s => s.Area)
                    .Include(s => s.EstadoSolicitud)
                    .Where(s => s.PersonaId == personaId)
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToListAsync();

                var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
                {
                    Id = s.Id,
                    NumeroSolicitud = s.NumeroSolicitud,
                    TipoSolicitud = s.TipoSolicitud,
                    FechaSolicitud = s.FechaSolicitud,
                    Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                    EstadoId = s.EstadoSolicitud?.Id ?? 0,
                    EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                    AreaNombre = s.Area?.Nombre ?? "No asignado",
                    PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                    UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener solicitudes: {ex.Message}" });
            }
        }

        [HttpGet("solicituddet/{id}")]
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

        [HttpGet("fotos-medicos/{fileName}")]
        public IActionResult GetFotoMedico(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FotosMedicos", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Archivo no encontrado");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "image/jpeg");
        }

        [HttpGet("documentos-educacion/{fileName}")]
        public IActionResult GetDocumentosEducacion(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EducacionDocumentos", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Archivo no encontrado");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf");
        }

        [HttpGet("solicitudesdets")]
        public async Task<ActionResult<SolicitudSeguimientoDto>> GetDetallesSolicitudes()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                EstadoId = s.EstadoSolicitud?.Id ?? 0,
                EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                AreaNombre = s.Area?.Nombre ?? "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            return Ok(resultado);
        }

        // Solo solicitudes en estado 1 (Registrado) para Consejo Regional
        [HttpGet("solicitudes-registradas")]
        public async Task<ActionResult<IEnumerable<SolicitudSeguimientoDto>>> GetSolicitudesRegistradas()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EstadoSolicitudId == 1)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                EstadoId = s.EstadoSolicitud?.Id ?? 0,
                EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                AreaNombre = s.Area?.Nombre ?? "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("solicitudes-aprobadas-cr")]
        public async Task<ActionResult<SolicitudSeguimientoDto>> GetSolicitudesAprobadasCR()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EstadoSolicitudId == 2) // Solo solicitudes aprobadas por Consejo Regional
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                EstadoId = s.EstadoSolicitud?.Id ?? 0,
                EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                AreaNombre = s.Area?.Nombre ?? "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            if (resultado == null || !resultado.Any())
                return NotFound(new { message = "No se encontraron solicitudes aprobadas por el Consejo Regional." });
            return Ok(resultado);
        }

        [HttpPost("cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromBody] SolicitudCambioEstadoDto dto)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona) // Incluye para acceder a Email
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

                if (solicitud is null)
                    return NotFound("Solicitud no encontrada");

                var estadoAnterior = solicitud.EstadoSolicitudId;

                // Cambiar estado
                solicitud.EstadoSolicitudId = dto.NuevoEstadoId;
                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Obtener el ID del usuario autenticado
                var usuarioId = GetUsuarioAutenticadoId();

                // Guardar en historial
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = dto.SolicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = dto.NuevoEstadoId,
                    FechaCambio = fechaCambio,
                    Observacion = dto.Observacion,
                    UsuarioCambio = usuarioId // Usar el ID del usuario autenticado
                };

                _context.SolicitudHistorialEstados.Add(historial);
                await _context.SaveChangesAsync();

                var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";

                var nombreEstado = await _context.EstadoSolicitudes
                    .Where(e => e.Id == dto.NuevoEstadoId)
                    .Select(e => e.Nombre)
                    .FirstOrDefaultAsync();

                await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, nombreEstado ?? "Sin Estado", dto.Observacion);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error al cambiar el estado: {ex.Message}");
            }
        }

        [HttpGet("EstadosConCheck/{personaId}")]
        public async Task<IActionResult> GetEstadosConCheck(int personaId)
        {
            // Obtener los estados con verReporte = true
            var estados = await _context.EstadoSolicitudes
                .Where(ed => ed.VerReporte)
                .Select(ed => new EstadoSolicitudConCheckDto
                {
                    Id = ed.Id,
                    Nombre = ed.Nombre,
                    Color = ed.Color,
                    TieneCheck = _context.Solicitudes
                        .Where(s => s.PersonaId == personaId)
                        .Join(_context.SolicitudHistorialEstados,
                            s => s.Id,
                            sd => sd.SolicitudId,
                            (s, sd) => sd)
                        .Any(sd => sd.EstadoNuevoId == ed.Id)
                })
                .ToListAsync();

            var historial = await (
                from s in _context.Solicitudes
                where s.PersonaId == personaId
                join e in _context.EstadoSolicitudes on s.EstadoSolicitudId equals e.Id
                select new
                {
                    SolicitudId = s.Id,
                    FechaSolicitud = s.FechaSolicitud,
                    EstadoId = e.Id,
                    EstadoNombre = e.Nombre,
                    EstadoColor = e.Color
                }
            ).ToListAsync();

            // Calcular el porcentaje
            var totalEstados = estados.Count;
            var estadosCompletados = estados.Count(e => e.TieneCheck);
            var porcentaje = totalEstados > 0 ? (estadosCompletados * 100.0 / totalEstados) : 0;

            // Construir la respuesta
            var response = new EstadoSolicitudConCheckResponse
            {
                PorcentajeCompletado = Math.Round(porcentaje, 2), // Redondea a 2 decimales
                NombreUltimoEstado = historial.FirstOrDefault()?.EstadoNombre,
                Estados = estados
            };

            return Ok(response);
        }
    }
}
