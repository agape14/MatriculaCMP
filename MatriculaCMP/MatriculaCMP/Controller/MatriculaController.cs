using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class MatriculaController : ControllerBase
	{
		private readonly MatriculaService _matriculaService;

		public MatriculaController(MatriculaService matriculaService)
		{
			_matriculaService = matriculaService;
		}

		[HttpPost("guardar")]
		public async Task<IActionResult> GuardarMatricula([FromForm] MatriculaRequest request)
		{
			try
			{
				var (success, message) = await _matriculaService.GuardarMatriculaAsync(
					request.Persona,
					request.Educacion,
					request.Foto,
					request.ResolucionFile);

				return success ? Ok(new { message }) : BadRequest(new { message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
			}
		}
	}
}
