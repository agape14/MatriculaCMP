using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UniversidadController : ControllerBase
    {
        private readonly UniversidadScraper _scraper;
        private readonly ApplicationDbContext _context;
        public UniversidadController(UniversidadScraper scraper, ApplicationDbContext context)
        {
            _scraper = scraper;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
			
            try
            {
                var universidades = await _context.Universidades
				.ToListAsync();

				if (universidades == null || universidades.Count == 0)
                {
                    return NotFound("No se encontraron universidades licenciadas.");
                }

                return Ok(universidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud."+ ex.Message);
            }
        }

		[HttpPost("llenar")]
		public async Task<IActionResult> LlenarTabla()
		{
			if (_context.Universidades.Any())
			{
				return Ok("La tabla de universidades ya contiene datos.");
			}

			await _scraper.ObtenerYGuardarUniversidadesAsync();
			return Ok("universidades insertados correctamente.");
		}

		[HttpGet("llenaruniversidades")]
		public async Task<IActionResult> LlenarTablaDesdeGet()
		{
			if (_context.Universidades.Any())
			{
				return Ok("La tabla de universidades ya contiene datos.");
			}

			await _scraper.ObtenerYGuardarUniversidadesAsync();
			return Ok("universidades insertados correctamente desde GET.");
		}

		[HttpGet("{paisId}")]
        public async Task<IActionResult> Get(int paisId)
        {
            var universidades = await _context.Universidades
                .Where(x => x.PaisId == paisId)
                .ToListAsync();
            return Ok(universidades);
        }
    }
}
