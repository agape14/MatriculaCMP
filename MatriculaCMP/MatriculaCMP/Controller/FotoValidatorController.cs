using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class FotoValidatorController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<FotoValidatorController> _logger;
		private readonly IConfiguration _config;
		
		// URL del microservicio Python
		private string PythonApiUrl => _config["ValidaimageIA:PythonApiUrl"] ?? "http://localhost:8026/validar-imagen";
		
		// Tamaño máximo de archivo configurable (por defecto 4MB)
		private long MaxFileSize => _config.GetValue<long>("ValidaimageIA:MaxFileSizeBytes", 4_194_304); // 4MB por defecto

		public FotoValidatorController(
			IHttpClientFactory httpClientFactory,
			ILogger<FotoValidatorController> logger,
			IConfiguration config)
		{
			_httpClient = httpClientFactory.CreateClient();
			_httpClient.Timeout = TimeSpan.FromSeconds(10); // Es mucho más rápido ahora
			_logger = logger;
			_config = config;
		}
		[HttpPost("validar")]
		public async Task<IActionResult> ValidarFoto([FromForm] IFormFile file)
		{
			try
			{
				// 1. Validaciones básicas de archivo
				var fileValidation = ValidateFile(file);
				if (fileValidation != null) return fileValidation;

				using var ms = new MemoryStream();
				await file.CopyToAsync(ms);
				var imageBytes = ms.ToArray();

				if (imageBytes.Length > MaxFileSize)
				{
					_logger.LogWarning("Imagen demasiado grande: {Size} bytes", imageBytes.Length);
					var maxSizeMB = MaxFileSize / (1024.0 * 1024.0);
					return BadRequest($"La imagen no debe exceder los {maxSizeMB:F1}MB");
				}

				// 2. Validación de dimensiones
				if (!ValidarDimensiones(imageBytes, out var dimensionError))
				{
					return Ok(new ValidationResult
					{
						Valido = false,
						Mensaje = dimensionError
					});
				}

				// 3. Llamada al microservicio Python
				var content = new MultipartFormDataContent();
				var fileContent = new ByteArrayContent(imageBytes);
				
				// Determinar el tipo de contenido basado en la extensión si ContentType está vacío
				var contentType = !string.IsNullOrWhiteSpace(file.ContentType) 
					? file.ContentType 
					: GetContentTypeFromExtension(file.FileName);
				
				fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
				content.Add(fileContent, "file", file.FileName);

				var response = await _httpClient.PostAsync(PythonApiUrl, content);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Error conectando con Python API: {StatusCode} - {ReasonPhrase}",
						response.StatusCode, response.ReasonPhrase);
					return StatusCode(500, new
					{
						Error = "El servicio de validación IA no está respondiendo",
						Detalles = $"Código: {response.StatusCode}"
					});
				}

				var pythonResponse = await response.Content.ReadFromJsonAsync<PythonValidationResponse>();
				if (pythonResponse == null)
				{
					_logger.LogError("Respuesta vacía del servicio Python");
					return StatusCode(500, new
					{
						Error = "Respuesta vacía del servicio de IA"
					});
				}

				// 4. Retornar resultado al frontend
				_logger.LogInformation("Validación completada - Válido: {Valido}, Motivo: {Motivo}",
					pythonResponse.Valido, pythonResponse.Motivo);

				return Ok(new ValidationResult
				{
					Valido = pythonResponse.Valido,
					Mensaje = pythonResponse.Valido
						? "Imagen válida correcta."
						: pythonResponse.Motivo,
					Detalles = pythonResponse.Detalles
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en FotoValidatorController");
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
				// Verificar que el servicio Python esté disponible
				// Intentamos hacer una petición HEAD o GET simple
				var healthUrl = PythonApiUrl.Replace("/validar-imagen", "/docs"); // FastAPI expone /docs por defecto
				var response = await _httpClient.GetAsync(healthUrl);
				
				return response.IsSuccessStatusCode
					? Ok(new { Status = "OK", PythonService = "Conectado", Url = PythonApiUrl })
					: StatusCode(503, new { Status = "Error", PythonService = "No responde" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al verificar estado del servicio Python");
				return StatusCode(503, new
				{
					Status = "Error",
					PythonService = "No se pudo conectar",
					Error = ex.Message,
					Url = PythonApiUrl
				});
			}
		}

		[HttpGet("config")]
		public IActionResult GetConfig()
		{
			try
			{
				var maxSizeMB = MaxFileSize / (1024.0 * 1024.0);
				var response = new
				{
					maxFileSizeBytes = MaxFileSize,
					maxFileSizeMB = maxSizeMB,
					maxFileSizeMBFormatted = $"{maxSizeMB:F1}"
				};
				_logger.LogInformation("Config endpoint llamado - MaxFileSizeBytes: {Bytes}, MaxFileSizeMB: {MB}", 
					MaxFileSize, maxSizeMB);
				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en GetConfig");
				return StatusCode(500, new { Error = "Error al obtener configuración" });
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

		private bool ValidarDimensiones(byte[] imgBytes, out string error)
		{
			error = string.Empty;
			try
			{
				using var stream = new MemoryStream(imgBytes);
				using var img = System.Drawing.Image.FromStream(stream);
				var min = Math.Min(img.Width, img.Height);
				if (min < 600)
				{
					error = $"La imagen es muy pequeña ({img.Width}x{img.Height}). Requiere al menos 600px en el lado menor.";
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				// Si falla la librería de imágenes, asumimos que está bien para que la IA decida
				_logger.LogWarning(ex, "Error al validar dimensiones, se continuará con la validación IA");
				return true;
			}
		}

		private string GetContentTypeFromExtension(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				return "image/jpeg";

			var extension = Path.GetExtension(fileName).ToLowerInvariant();
			return extension switch
			{
				".jpg" or ".jpeg" => "image/jpeg",
				".png" => "image/png",
				_ => "image/jpeg" // Por defecto
			};
		}
		#endregion
		#region Clases de Soporte
		public class ValidationResult
		{
			public bool Valido { get; set; }
			public string? Mensaje { get; set; }
			public object? Detalles { get; set; }
		}

		private class PythonValidationResponse
		{
			[JsonPropertyName("valido")]
			public bool Valido { get; set; }

			[JsonPropertyName("motivo")]
			public string Motivo { get; set; } = string.Empty;

			[JsonPropertyName("detalles")]
			public object? Detalles { get; set; }
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