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
				return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
			}
		}

        [HttpGet("mis-solicitudes/{personaId:int}")]
        public async Task<IActionResult> ObtenerSolicitudesUsuario(int personaId)
        {
            var solicitudes = await _context.Solicitudes
            .Where(s => s.PersonaId == personaId)
            .Include(s => s.Persona)
                .ThenInclude(p => p.Educaciones)
                    .ThenInclude(e => e.Universidad)
            .Include(s => s.Area)
            .Include(s => s.EstadoSolicitud)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync(); // <-- fuerza la ejecución en memoria

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

        [HttpGet("solicituddet/{id}")]
        public async Task<ActionResult<Solicitud>> GetDetalleSolicitud(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)       
                .Include(s => s.Persona)
                    .ThenInclude(p => p.GrupoSanguineo)
                .Include(s => s.EstadoSolicitud)
                .Include(s => s.HistorialEstados)
                .Include(s => s.Persona)
                    .ThenInclude(e => e.Educaciones)
                    .ThenInclude(d => d.Documento)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            return Ok(solicitud);
        }

        [HttpGet("fotos-medicos/{fileName}")]
        public IActionResult GetFotoMedico(string fileName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FotosMedicos", fileName);

            if (!System.IO.File.Exists(imagePath))
                return NotFound();

            return PhysicalFile(imagePath, "image/jpeg");
        }

        [HttpGet("documentos-educacion/{fileName}")]
        public IActionResult GetDocumentosEducacion(string fileName)
        {
            var documentoPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EducacionDocumentos", fileName);

            if (!System.IO.File.Exists(documentoPath))
                return NotFound();

            return PhysicalFile(documentoPath, "application/pdf");
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
            .Where(s => s.EstadoSolicitudId ==1) // Asegurarse de que el estado no sea nulo
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync(); // <-- fuerza la ejecución en memoria

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

            if (resultado == null || !resultado.Any())
                return NotFound(new { message = "No se encontraron solicitudes." });
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

                // Guardar en historial
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = dto.SolicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = dto.NuevoEstadoId,
                    FechaCambio = DateTime.UtcNow,
                    Observacion = dto.Observacion,
                    UsuarioCambio = dto.UsuarioCambio
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

                await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, nombreEstado ?? "Sin Estado");

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

            // Calcular el porcentaje
            var totalEstados = estados.Count;
            var estadosCompletados = estados.Count(e => e.TieneCheck);
            var porcentaje = totalEstados > 0 ? (estadosCompletados * 100.0 / totalEstados) : 0;

            // Construir la respuesta
            var response = new EstadoSolicitudConCheckResponse
            {
                PorcentajeCompletado = Math.Round(porcentaje, 2), // Redondea a 2 decimales
                Estados = estados
            };

            return Ok(response);
        }
    }
}
