using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsejoRegionalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsejoRegionalController(ApplicationDbContext context)
        {
            _context = context;
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
				var consejos = await _context.Mat_ConsejoRegional
	                .Where(c => c.Activo == true)
	                .OrderBy(c => c.Nombre)
	                .Select(c => new Mat_ConsejoRegional
					{
		                ConsejoRegional_Key = c.ConsejoRegional_Key,
		                Nombre = c.Nombre
	                })
	                .ToListAsync();

				return Ok(consejos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al obtener los consejos regionales.",
                    detalle = ex.Message
                });
            }
        }
    }
}
