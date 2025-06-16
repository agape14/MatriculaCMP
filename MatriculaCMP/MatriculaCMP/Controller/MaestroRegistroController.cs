using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class MaestroRegistroController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public MaestroRegistroController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet("id/{tablaKey}")]
		public async Task<IActionResult> GetPorTabla(int tablaKey)
		{
			try
			{
				var resultado = await _context.MaestroRegistro
					.Where(m => m.MaestroTabla_Key == tablaKey)
					.OrderBy(m => m.Nombre)
					.Select(m => new MaestroRegistro
					{
						MaestroRegistro_Key = m.MaestroRegistro_Key,
						Codigo = m.Codigo!,
						Nombre = m.Nombre!
					})
					.ToListAsync();

				return Ok(resultado);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					mensaje = "Error al obtener registros del maestro.",
					detalle = ex.Message
				});
			}
		}
	}
}
