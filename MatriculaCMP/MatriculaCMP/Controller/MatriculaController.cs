using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

	[HttpPost("crear-inicial")]
	public async Task<IActionResult> CrearSolicitudInicial(
		[FromForm] string Persona,
		[FromForm] string Educacion,
		[FromForm] string? PerfilSeleccionado = null,
		[FromForm] IFormFile? CarnetExtranjeria = null)
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

			var user = HttpContext.User;
			var docsPdf = new Dictionary<string, IFormFile?>
			{
				["CarnetExtranjeria"] = CarnetExtranjeria
			};

			var (success, message) = await _matriculaService.GuardarMatriculaAsync(
				persona,
				educacion,
				null,
				null,
				docsPdf,
				null,
				user,
				esRegistroInicial: true,
				perfilSeleccionado: PerfilSeleccionado);

				if (success)
				{
					// Extraer el ID de la solicitud del mensaje si está presente
					// El servicio debe retornar algo como "Solicitud creada con ID: 123"
					int? solicitudId = null;
					if (message.Contains("ID:") || message.Contains("Id:"))
					{
						var parts = message.Split(new[] { "ID:", "Id:" }, StringSplitOptions.None);
						if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out var id))
						{
							solicitudId = id;
						}
					}
					return Ok(new { success = true, message = message, solicitudId = solicitudId });
				}
				
				return Ok(new { success = false, message = message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
			}
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

				// Obtener el usuario autenticado
				var user = HttpContext.User;

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
					},
					null, // solicitudId para nueva solicitud
					user); // Pasar el usuario autenticado

				return success ? Ok(new { success = true, message = message }) : Ok(new { success = false, message = message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
			}
		}

        [HttpPost("editar/{solicitudId}")]
        public async Task<IActionResult> EditarMatricula(
			int solicitudId,
			[FromForm] string Persona,
			[FromForm] string Educacion,
			[FromForm] string? PerfilSeleccionado = null,
			[FromForm] IFormFile? Foto = null, // Hacer opcional para edición
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
                // Deserializar objetos
                var persona = JsonSerializer.Deserialize<Persona>(Persona);
                var educacion = JsonSerializer.Deserialize<Educacion>(Educacion);

                // Obtener el usuario autenticado
                var user = HttpContext.User;

                // Preparar diccionario de documentos
                var docsPdf = new Dictionary<string, IFormFile?>
                {
                    ["TituloMedicoCirujano"] = TituloMedicoCirujano,
                    ["ConstanciaInscripcionSunedu"] = ConstanciaInscripcionSunedu,
                    ["CertificadoAntecedentesPenales"] = CertificadoAntecedentesPenales,
                    ["CarnetExtranjeria"] = CarnetExtranjeria,
                    ["ConstanciaInscripcionReconocimientoSunedu"] = ConstanciaInscripcionReconocimientoSunedu,
                    ["ConstanciaInscripcionRevalidacionUniversidadNacional"] = ConstanciaInscripcionRevalidacionUniversidadNacional,
                    ["ReconocimientoSunedu"] = ReconocimientoSunedu,
                    ["RevalidacionUniversidadNacional"] = RevalidacionUniversidadNacional
                };

                var (success, message) = await _matriculaService.GuardarMatriculaAsync(
                    persona,
                    educacion,
                    Foto,
                    ResolucionFile,
                    docsPdf,
                    solicitudId, // Pasar el ID de solicitud existente
                    user, // Pasar el usuario autenticado
                    esRegistroInicial: false, // No es registro inicial, es actualización
                    perfilSeleccionado: PerfilSeleccionado); // Pasar perfil si existe

                if (success)
                {
                    // Extraer el ID de la solicitud del mensaje si está presente
                    int? solicitudIdFromMessage = solicitudId; // Usar el ID que ya tenemos
                    if (message.Contains("ID:") || message.Contains("Id:"))
                    {
                        var parts = message.Split(new[] { "ID:", "Id:" }, StringSplitOptions.None);
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out var id))
                        {
                            solicitudIdFromMessage = id;
                        }
                    }
                    return Ok(new { success = true, message = message, solicitudId = solicitudIdFromMessage });
                }
                
                return Ok(new { success = false, message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
            }
        }
    }
}
