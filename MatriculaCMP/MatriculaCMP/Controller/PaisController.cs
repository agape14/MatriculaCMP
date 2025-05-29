using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaisController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
		private readonly PaisesService _paisesService;
		public PaisController(ApplicationDbContext context, PaisesService paisesService)
		{
			_context = context;
			_paisesService = paisesService;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			try
			{
				var paises = await _context.Paises
					.OrderBy(p => p.Nombre)
					.ToListAsync();

				return Ok(paises);
			}
			catch (Exception ex)
			{
				// Puedes loguear el error si tienes un sistema de logging configurado
				// _logger.LogError(ex, "Error al obtener la lista de países");

				return StatusCode(500, new
				{
					mensaje = "Ocurrió un error al obtener la lista de países.",
					detalle = ex.Message
				});
			}
		}

		[HttpPost("llenar")]
		public async Task<IActionResult> LlenarTabla()
		{
			if (_context.Paises.Any())
			{
				return Ok("La tabla de países ya contiene datos.");
			}

			await _paisesService.ObtenerYPersistirPaisesAsync();
			return Ok("Países insertados correctamente.");
		}

		[HttpGet("llenarpaises")]
		public async Task<IActionResult> LlenarTablaDesdeGet()
		{
			if (_context.Paises.Any())
			{
				return Ok("La tabla de países ya contiene datos.");
			}

			await _paisesService.ObtenerYPersistirPaisesAsync();
			return Ok("Países insertados correctamente desde GET.");
		}

	}
}
