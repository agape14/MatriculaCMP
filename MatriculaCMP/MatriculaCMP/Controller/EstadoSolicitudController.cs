using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoSolicitudController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoSolicitudController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<EstadoSolicitud>>> Get()
        {
            return await _context.EstadoSolicitudes.ToListAsync();
        }
    }
}
