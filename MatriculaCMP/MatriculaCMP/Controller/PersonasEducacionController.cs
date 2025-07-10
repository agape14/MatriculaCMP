using MatriculaCMP.Server.Data;
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
            var persona = await _context.Personas
                .Include(p => p.Educaciones)
                .Include(p => p.Usuarios)
                //.Include(p => p.Solicitudes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (persona == null)
                return NotFound(new { message = $"No se encontró la persona con ID {id}" });

            return Ok(persona);
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
                    .ThenInclude(p => p.GrupoSanguineo) // 👈 aquí se incluye el grupo sanguíneo
                .Include(s => s.EstadoSolicitud)
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

        [HttpGet("solicitudesdets")]
        public async Task<ActionResult<SolicitudSeguimientoDto>> GetDetallesSolicitudes()
        {
            //var solicitud = await _context.Solicitudes
            //    .Include(s => s.Persona)
            //        .ThenInclude(p => p.Educaciones)
            //            .ThenInclude(e => e.Universidad)
            //    .Include(s => s.Persona)
            //        .ThenInclude(p => p.GrupoSanguineo) // 👈 aquí se incluye el grupo sanguíneo
            //    .Include(s => s.EstadoSolicitud)
            //    .ToListAsync();

            //if (solicitud == null)
            //    return NotFound();



            var solicitudes = await _context.Solicitudes
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

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al cambiar el estado");
            }
        }

    }
}
