using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class FotoValidatorController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<FotoValidatorController> _logger;
		private const string OllamaBaseUrl = "http://localhost:11434/api/generate";
		private const string ModelName = "llava:7b";
		private const int MaxFileSize = 4_000_000; // 4MB

		public FotoValidatorController(
			IHttpClientFactory httpClientFactory,
			ILogger<FotoValidatorController> logger)
		{
			_httpClient = httpClientFactory.CreateClient();
			_logger = logger;
			_httpClient.Timeout = TimeSpan.FromSeconds(30); // Timeout de 30 segundos
		}

		[HttpPost("validar")]
		[RequestSizeLimit(5_000_000)]
		public async Task<IActionResult> ValidarFoto([FromForm] IFormFile file)
		{
			try
			{
				// Validación inicial del archivo
				var fileValidation = ValidateFile(file);
				if (fileValidation != null) return fileValidation;

				using var ms = new MemoryStream();
				await file.CopyToAsync(ms);
				var imageBytes = ms.ToArray();

				if (imageBytes.Length > MaxFileSize)
				{
					_logger.LogWarning($"Imagen demasiado grande: {imageBytes.Length} bytes");
					return BadRequest("La imagen no debe exceder los 4MB");
				}

				var base64Image = Convert.ToBase64String(imageBytes);

				// 1. Análisis de género
				var generoPrompt = "Responde en español. Analiza la imagen y responde. " +
								 "¿La imagen muestra claramente a un hombre(varon) o una mujer(dama)?";

				var generoAnalysis = await AnalyzeImage(base64Image, generoPrompt);
				var genero = ParseGeneroResponse(generoAnalysis.Response);

				if (genero == "indeterminado")
				{
					_logger.LogInformation("No se pudo determinar el género: {Response}", generoAnalysis.Response);
					return Ok(new ValidationResult
					{
						Valido = false,
						Mensaje = "No se pudo determinar el género",
						Detalles = new
						{
							Genero = genero,
							RespuestaIA = generoAnalysis.Response,
							RawResponse = generoAnalysis.RawResponse
						}
					});
				}

				// 2. Validación de requisitos
				var validationPrompt = genero == "hombre"
					? BuildMaleValidationPrompt()
					: BuildFemaleValidationPrompt();

				var validationAnalysis = await AnalyzeImage(base64Image, validationPrompt);
				var esValido = ValidateResponse(validationAnalysis.Response);

				_logger.LogInformation("Validación completada - Género: {Genero}, Válido: {Valido}, Respuesta: {Response}",
					genero, esValido, validationAnalysis.Response);

				return Ok(new ValidationResult
				{
					Valido = esValido,
					Genero = genero,
					Mensaje = esValido
						? "Imagen válida cumple con todos los requisitos"
						: "La imagen no cumple con los requisitos",
					Detalles = new
					{
						RespuestaIA = validationAnalysis.Response,
						RawResponse = validationAnalysis.RawResponse,
						PromptUsado = validationPrompt
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al validar la imagen");
				return StatusCode(500, new
				{
					Error = "Ocurrió un error interno",
					Detalles = ex.Message
				});
			}
		}

		[HttpGet("status")]
		public async Task<IActionResult> Status()
		{
			try
			{
				var response = await _httpClient.GetAsync(OllamaBaseUrl.Replace("/generate", "/tags"));
				return response.IsSuccessStatusCode
					? Ok(new { Status = "OK", Ollama = "Conectado" })
					: StatusCode(503, new { Status = "Error", Ollama = "No responde" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al verificar estado de Ollama");
				return StatusCode(503, new
				{
					Status = "Error",
					Ollama = "No se pudo conectar",
					Error = ex.Message
				});
			}
		}

		#region Métodos Privados

		private IActionResult? ValidateFile(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				_logger.LogWarning("Intento de validación con archivo vacío");
				return BadRequest("Debe proporcionar un archivo de imagen válido");
			}

			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
			var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(fileExtension))
			{
				_logger.LogWarning("Tipo de archivo no permitido: {Extension}", fileExtension);
				return BadRequest($"Solo se permiten imágenes en formato {string.Join(", ", allowedExtensions)}");
			}

			return null;
		}

		private async Task<AnalysisResult> AnalyzeImage(string base64Image, string prompt)
		{
			var request = new
			{
				model = ModelName,
				prompt,
				images = new[] { base64Image },
				stream = false,
				options = new { temperature = 0.2 }
			};

			var requestContent = new StringContent(
				JsonSerializer.Serialize(request),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(OllamaBaseUrl, requestContent);
			var rawResponse = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Error en la API Ollama: {StatusCode} - {Response}",
					response.StatusCode, rawResponse);
				return new AnalysisResult
				{
					Response = null,
					RawResponse = rawResponse
				};
			}

			using var doc = JsonDocument.Parse(rawResponse);
			var responseText = doc.RootElement.TryGetProperty("response", out var res)
				? res.GetString()?.Trim()
				: null;

			return new AnalysisResult
			{
				Response = responseText,
				RawResponse = rawResponse
			};
		}

		private string ParseGeneroResponse(string? response)
		{
			if (string.IsNullOrWhiteSpace(response))
				return "indeterminado";

			var cleanResponse = response.ToLowerInvariant().Trim();

			if (cleanResponse.Contains("hombre")) return "hombre";
			if (cleanResponse.Contains("mujer")) return "mujer";

			return "indeterminado";
		}

		private bool ValidateResponse(string? response)
		{
			if (string.IsNullOrWhiteSpace(response))
				return false;

			var cleanResponse = response.ToLowerInvariant().Trim();
			return cleanResponse.StartsWith("sí") || cleanResponse.StartsWith("si");
		}

		private string BuildMaleValidationPrompt()
		{
			return @"Analiza la imagen y responde únicamente 'sí' o 'no' considerando estos requisitos:
			1. Fondo blanco liso
			2. Persona vistiendo terno formal con corbata
			3. Rostro claramente visible
			4. Postura frontal
			5. Sin accesorios inapropiados

			¿Cumple con todos estos requisitos?";
		}

		private string BuildFemaleValidationPrompt()
		{
			return @"Analiza la imagen y responde únicamente 'sí' o 'no' considerando estos requisitos:
			1. Fondo blanco liso
			2. Persona vistiendo blusa formal o traje de oficina
			3. Rostro claramente visible
			4. Postura frontal
			5. Sin accesorios inapropiados

			¿Cumple con todos estos requisitos?";
		}

		#endregion

		#region Clases de Soporte

		private class AnalysisResult
		{
			public string? Response { get; set; }
			public string? RawResponse { get; set; }
		}

		public class ValidationResult
		{
			public bool Valido { get; set; }
			public string? Genero { get; set; }
			public string? Mensaje { get; set; }
			public object? Detalles { get; set; }
		}

		[HttpGet("model-info")]
		public async Task<IActionResult> GetModelInfo()
		{
			try
			{
				var response = await _httpClient.GetAsync(OllamaBaseUrl.Replace("/generate", "/tags"));
				if (!response.IsSuccessStatusCode)
				{
					return StatusCode(503, new
					{
						Status = "Ollama no responde",
						Error = response.StatusCode
					});
				}

				var models = await response.Content.ReadFromJsonAsync<object>();
				return Ok(new
				{
					Status = "OK",
					ModeloSolicitado = ModelName,
					ModelosDisponibles = models
				});
			}
			catch (Exception ex)
			{
				return StatusCode(503, new
				{
					Status = "Error",
					Error = ex.Message
				});
			}
		}
		#endregion
	}
}