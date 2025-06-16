using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class MatPaisController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

        public MatPaisController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			try
			{
				var consejos = await _context.MatPaises
					.Where(c => c.Activo == true)
					.OrderBy(c => c.Nombre)
					.Select(c => new Mat_Pais
					{
						Pais_key = c.Pais_key,
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
