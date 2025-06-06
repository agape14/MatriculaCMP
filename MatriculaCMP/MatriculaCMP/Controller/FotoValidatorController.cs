using Microsoft.AspNetCore.Mvc;
using System.Resources;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class FotoValidatorController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<FotoValidatorController> _logger;
        private readonly IConfiguration _config;
		private const string ModelName = "llava:7b";
		private const int MaxFileSize = 4_000_000; // 4MB
		public FotoValidatorController(
			IHttpClientFactory httpClientFactory,
			ILogger<FotoValidatorController> logger, IConfiguration config)
		{
			_httpClient = httpClientFactory.CreateClient();
			_logger = logger;
			_httpClient.Timeout = TimeSpan.FromSeconds(50); // Timeout de 30 segundos
            _config = config;
            
        }
		[HttpPost("validar")]
		[RequestSizeLimit(5_000_000)]
		public async Task<IActionResult> ValidarFoto([FromForm] IFormFile file)
		{
			try
			{
				// 1. Reiniciar Ollama antes de cada análisis
				await ResetModel();

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
				var generoPrompt = "Responde en el idioma español. Analiza la imagen y responde. " +
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
                var OllamaBaseUrl = _config["ValidaimageIA:LinkIA"];
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
            var OllamaBaseUrl = _config["ValidaimageIA:LinkIA"];
            var request = new
			{
				model = ModelName,
				prompt,
				images = new[] { base64Image },
				stream = false,
				options = new
				{
					temperature = 0,       // Máxima determinación
					top_k = 1,            // Solo considerar la opción más probable
					num_ctx = 2048,       // Contexto amplio
					seed = 42             // Semilla fija para reproducibilidad
				}
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

			// Patrones más estrictos para determinar género
			if (Regex.IsMatch(cleanResponse, @"\b(hombre|varón|masculino)\b"))
				return "hombre";
			if (Regex.IsMatch(cleanResponse, @"\b(mujer|dama|femenino)\b"))
				return "mujer";

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
			return @"Evalúa esta imagen como VALIDA para matrícula profesional SOLO si cumple TODOS estos requisitos (responde EXCLUSIVAMENTE 'sí' o 'no'):

			1. [FONDO] Color claro (blanco o tono claro uniforme) sin patrones ni objetos visibles
			2. [VESTIMENTA] Atuendo formal (terno/con corbata o camisa formal de manga larga)
			3. [POSTURA] Rostro frontal con mirada al frente (se permite ligera inclinación natural)
			4. [ROSTRO] Claramente visible sin obstrucciones (anteojos permitidos si no reflejan luz)
			5. [COMPOSICIÓN] Una sola persona ocupando al menos el 60% de la imagen

			Consideraciones adicionales:
			- Se permiten sombras suaves naturales
			- Barba/cabello bien arreglados son aceptables
			- Corbata preferible pero no obligatoria si la vestimenta es formal

			¿Cumple TODOS los requisitos esenciales?";
		}

		private string BuildFemaleValidationPrompt()
		{
			return @"Evalúa esta imagen como VALIDA para matrícula profesional SOLO si cumple TODOS estos requisitos (responde EXCLUSIVAMENTE 'sí' o 'no'):

			1. [FONDO] Color claro (blanco o tono claro uniforme) sin patrones ni objetos visibles
			2. [VESTIMENTA] Atuendo formal (blusa/chaqueta profesional o vestido formal)
			3. [POSTURA] Rostro frontal con mirada al frente (se permite ligera inclinación natural)
			4. [ROSTRO] Claramente visible sin obstrucciones (maquillaje discreto permitido)
			5. [COMPOSICIÓN] Una sola persona ocupando al menos el 60% de la imagen

			Consideraciones adicionales:
			- Se permiten aretes pequeños y collar discreto
			- Cabello recogido o suelto mientras no tape el rostro
			- Blusa con escote moderado aceptable

			¿Cumple TODOS los requisitos esenciales?";
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
            var OllamaBaseUrl = _config["ValidaimageIA:LinkIA"];
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

		private async Task RestartOllamaService()
		{
			try
			{
				// Ejecutar comando para reiniciar Ollama (depende del sistema operativo)
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					var process = new Process
					{
						StartInfo = new ProcessStartInfo
						{
							FileName = "cmd.exe",
							Arguments = "/c net stop ollama && net start ollama",
							CreateNoWindow = true,
							UseShellExecute = false
						}
					};
					process.Start();
					await process.WaitForExitAsync();
				}
				else // Linux/Mac
				{
					var process = new Process
					{
						StartInfo = new ProcessStartInfo
						{
							FileName = "/bin/bash",
							Arguments = "-c \"pkill -f ollama && ollama serve &\"",
							CreateNoWindow = true,
							UseShellExecute = false
						}
					};
					process.Start();
					await process.WaitForExitAsync();
				}

				// Esperar 2 segundos para que el servicio se reinicie completamente
				await Task.Delay(2000);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al reiniciar Ollama");
			}
		}

		[HttpPost("reset-model")]
		public async Task<IActionResult> ResetModel()
		{
            var OllamaBaseUrl = _config["ValidaimageIA:LinkIA"];
            var resetRequest = new
			{
				model = ModelName,
				prompt = "/reset",
				stream = false
			};

			var response = await _httpClient.PostAsJsonAsync(OllamaBaseUrl, resetRequest);
			return response.IsSuccessStatusCode
				? Ok(new { Status = "Modelo reseteado" })
				: StatusCode(503, new { Error = "No se pudo resetear el modelo" });
		}
		#endregion

		[HttpGet("reference-image/{imageName}")]
		public IActionResult GetReferenceImage(string imageName)
		{
			var validImages = new[] { "hombre_formal.jpg", "mujer_formal.jpg", "fondo_blanco.jpg", "ejemplo_rechazado.jpg" };

			if (!validImages.Contains(imageName))
				return NotFound();

			// Obtener la ruta base del contenido (wwwroot)
			var contentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "ReferenceImages");
			var imagePath = Path.Combine(contentRootPath, imageName);

			if (!System.IO.File.Exists(imagePath))
				return NotFound();

			return PhysicalFile(imagePath, "image/jpeg");
		}
	}
}