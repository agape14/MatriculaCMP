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

        [HttpGet("mis-solicitudes/{persona_id}")]
        public async Task<IActionResult> ObtenerSolicitudesUsuario(string persona_id)
        {
            var solicitudes = await _context.Solicitudes
                .Where(s => s.PersonaId.ToString() == persona_id)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => new SolicitudSeguimientoDto
                {
                    Id = s.Id,
                    TipoSolicitud = s.TipoSolicitud,
                    FechaSolicitud = s.FechaSolicitud,
                    Estado = s.EstadoSolicitud.Nombre,
                    AreaNombre = s.Area != null ? s.Area.Nombre : "No asignado",
                    Observaciones = s.Observaciones
                })
                .ToListAsync();

            return Ok(solicitudes);
        }

    }
}
