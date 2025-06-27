using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
    [Route("api/[controller]")]          // api/perfiles
    [ApiController]
    public class PerfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PerfilesController> _logger;

        public PerfilesController(ApplicationDbContext db,
                                  ILogger<PerfilesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET api/perfiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Perfil>>> Get()
        {
            try
            {
                var lista = await _db.Perfil
                                     .AsNoTracking()
                                     .ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfiles");
                return StatusCode(500, "Ocurrió un error al consultar los perfiles");
            }
        }

        // GET api/perfiles/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Perfil>> Get(int id)
        {
            try
            {
                var perfil = await _db.Perfil
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(p => p.Id == id);

                return perfil is null ? NotFound() : Ok(perfil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil {Id}", id);
                return StatusCode(500, "Ocurrió un error al consultar el perfil");
            }
        }

        // POST api/perfiles
        [HttpPost]
        public async Task<IActionResult> Post(Perfil p)
        {
            try
            {
                _db.Perfil.Add(p);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
            }
            catch (DbUpdateException dbx)
            {
                _logger.LogWarning(dbx, "Error de BD al crear perfil");
                return BadRequest("No se pudo registrar el perfil");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear perfil");
                return StatusCode(500, "Ocurrió un error al registrar el perfil");
            }
        }

        // PUT api/perfiles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Perfil p)
        {
            if (id != p.Id) return BadRequest("Id del cuerpo y de la URL no coinciden");

            try
            {
                _db.Entry(p).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException cx)
            {
                _logger.LogWarning(cx, "Conflicto de concurrencia al actualizar {Id}", id);
                return NotFound("El perfil fue eliminado o actualizado por otro proceso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar {Id}", id);
                return StatusCode(500, "Ocurrió un error al actualizar el perfil");
            }
        }

        // DELETE api/perfiles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var perfil = await _db.Perfil.FindAsync(id);
                if (perfil is null) return NotFound();

                _db.Perfil.Remove(perfil);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar {Id}", id);
                return StatusCode(500, "Ocurrió un error al eliminar el perfil");
            }
        }
    }
}
