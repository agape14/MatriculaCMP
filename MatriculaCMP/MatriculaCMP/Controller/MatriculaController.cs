using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
		public async Task<IActionResult> GuardarMatricula(
			[FromForm] string Persona,
			[FromForm] string Educacion,
			[FromForm] IFormFile Foto,
			[FromForm] IFormFile? ResolucionFile = null,
			[FromForm] IFormFile? TituloMedicoCirujano = null,
			[FromForm] IFormFile? ConstanciaInscripcionSunedu = null,
			[FromForm] IFormFile? CertificadoAntecedentesPenales = null,
			[FromForm] IFormFile? CarnetExtranjeria = null,
			[FromForm] IFormFile? ConstanciaInscripcionReconocimientoSunedu = null,
			[FromForm] IFormFile? ConstanciaInscripcionRevalidacionUniversidadNacional = null,
			[FromForm] IFormFile? ReconocimientoSunedu = null,
			[FromForm] IFormFile? RevalidacionUniversidadNacional = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(Persona) || string.IsNullOrWhiteSpace(Educacion))
				{
					return BadRequest(new { message = "Los campos 'Persona' y 'Educacion' son obligatorios." });
				}

				Persona? persona;
				Educacion? educacion;

				try
				{
					persona = JsonSerializer.Deserialize<Persona>(Persona);
					educacion = JsonSerializer.Deserialize<Educacion>(Educacion);
				}
				catch (JsonException jsonEx)
				{
					return BadRequest(new { message = $"Error de formato JSON: {jsonEx.Message}" });
				}

				if (persona == null || educacion == null)
				{
					return BadRequest(new { message = "No se pudo deserializar los datos de 'Persona' o 'Educacion'." });
				}

				// Eliminado el guardado de archivos aquí
				var (success, message) = await _matriculaService.GuardarMatriculaAsync(
					persona,
					educacion,
					Foto, // Pasamos el IFormFile directamente
					ResolucionFile,
					new Dictionary<string, IFormFile?>
					{
						["TituloMedicoCirujano"] = TituloMedicoCirujano,
						["ConstanciaInscripcionSunedu"] = ConstanciaInscripcionSunedu,
						["CertificadoAntecedentesPenales"] = CertificadoAntecedentesPenales,
						["CarnetExtranjeria"] = CarnetExtranjeria,
						["ConstanciaInscripcionReconocimientoSunedu"] = ConstanciaInscripcionReconocimientoSunedu,
						["ConstanciaInscripcionRevalidacionUniversidadNacional"] = ConstanciaInscripcionRevalidacionUniversidadNacional,
						["ReconocimientoSunedu"] = ReconocimientoSunedu,
						["RevalidacionUniversidadNacional"] = RevalidacionUniversidadNacional
					});

				return success ? Ok(new { success = true, message = message }) : Ok(new { success = false, message = message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
			}
		}
	}
}
