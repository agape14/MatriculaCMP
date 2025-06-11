using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
