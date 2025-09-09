using Microsoft.AspNetCore.Mvc;
using System.Resources;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class FotoValidatorController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<FotoValidatorController> _logger;
		private readonly IConfiguration _config;
		private const string LlavaModel = "llava:7b";
		private const string MoondreamModel = "moondream:latest";
		private static readonly SemaphoreSlim LlavaGate = new SemaphoreSlim(1, 1);
		private static readonly SemaphoreSlim MoondreamGate = new SemaphoreSlim(3, 3); // más liviano, permitir 3 en paralelo
		private const int MaxFileSize = 4_000_000; // 4MB
		public FotoValidatorController(
			IHttpClientFactory httpClientFactory,
			ILogger<FotoValidatorController> logger, IConfiguration config)
		{
			_httpClient = httpClientFactory.CreateClient();
			_logger = logger;
			_httpClient.Timeout = TimeSpan.FromSeconds(120); // aumentar timeout para primera inferencia
			_config = config;
			
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

				// Chequeo mínimo de dimensiones (lado menor ≥ 600px) para estilo carnet/pasaporte
				using (var imgStream = new MemoryStream(imageBytes))
				using (var img = System.Drawing.Image.FromStream(imgStream))
				{
					var minSide = Math.Min(img.Width, img.Height);
					if (minSide < 600)
					{
						return Ok(new ValidationResult
						{
							Valido = false,
							Genero = null,
							Mensaje = $"La imagen es muy pequeña ({img.Width}x{img.Height}). Requiere al menos 600px en el lado menor.",
							Detalles = new { Ancho = img.Width, Alto = img.Height, LadoMenor = minSide, Requerido = 600, Capa = "PreValidacion" }
						});
					}
				}

				var base64Image = Convert.ToBase64String(imageBytes);

				// Capa 1: Moondream (filtro rápido y ligero) con salida estructurada
				var clipPrompt = "Devuelve SOLO este formato exacto: genero:(hombre|mujer|desconocido); fondo:(blanco|no blanco); selfie:(si|no); vestimenta:(formal|informal); postura:(frontal|no frontal); rostro:(visible|no visible); corbata:(si|no|na).";
				await MoondreamGate.WaitAsync();
				AnalysisResult clip;
				try
				{
					clip = await AnalyzeImage(base64Image, clipPrompt, MoondreamModel);
				}
				finally
				{
					MoondreamGate.Release();
				}
				var genero = ParseGeneroResponse(clip.Response);

				// Reglas rápidas de descarte negativo
				if (ClipDescarta(clip.Response))
				{
					return Ok(new ValidationResult
					{
						Valido = false,
						Genero = genero == "indeterminado" ? null : genero,
						Mensaje = "La imagen no cumple requisitos básicos (fondo/ropa/selfie)",
						Detalles = new { Capa = "Moondream", RespuestaIA = clip.Response, Raw = clip.RawResponse }
					});
				}

				// Intento de aceptación rápida si cumple todo
				var quick = ParseQuickChecks(clip.Response);
				if (quick != null &&
					quick.FondoBlanco && quick.RostroVisible && quick.PosturaFrontal && quick.VestimentaFormal && !quick.EsSelfie &&
					((quick.Genero == "hombre" && quick.Corbata == true) || quick.Genero == "mujer"))
				{
					return Ok(new ValidationResult
					{
						Valido = true,
						Genero = quick.Genero,
						Mensaje = "Imagen válida cumple con todos los requisitos (validación rápida)",
						Detalles = new { Capa = "Moondream", RespuestaIA = clip.Response, Raw = clip.RawResponse, Quick = quick }
					});
				}

				// Capa 2: LLaVA (validación profunda) con control de concurrencia
				await LlavaGate.WaitAsync();
				AnalysisResult validationAnalysis;
				string validationPrompt;
				try
				{
					validationPrompt = genero == "hombre" ? BuildMaleValidationPrompt() : BuildFemaleValidationPrompt();
					validationAnalysis = await AnalyzeImage(base64Image, validationPrompt, LlavaModel);
				}
				finally
				{
					LlavaGate.Release();
				}
				var (esValido, motivo) = EvaluateResponse(validationAnalysis.Response);

				if (esValido)
				{
					_logger.LogInformation("Validación completada - Género: {Genero}, Válido: {Valido}",
						genero, esValido);
				}
				else
				{
					_logger.LogWarning("Validación rechazada - Motivo: {Motivo}. Género: {Genero}. Respuesta: {Response}",
						motivo, genero, validationAnalysis.Response);
				}

				return Ok(new ValidationResult
				{
					Valido = esValido,
					Genero = genero,
					Mensaje = esValido
						? "Imagen válida cumple con todos los requisitos"
						: ($"La imagen no cumple con los requisitos. Motivo: {motivo}"),
					Detalles = new
					{
						CapaRapida = "Moondream",
						RespuestaMoondream = clip.Response,
						RawMoondream = clip.RawResponse,
						Quick = quick,
						ModeloProfundo = LlavaModel,
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
		private async Task<AnalysisResult> AnalyzeImage(string base64Image, string prompt, string model)
		{
			var OllamaBaseUrl = _config["ValidaimageIA:LinkIA"];
			object options;
			if (model == MoondreamModel)
			{
				options = new
				{
					temperature = 0,
					top_k = 1,
					num_ctx = 512,
					num_predict = 96,
					seed = 42,
					keep_alive = "1m"
				};
			}
			else // LLaVA u otros
			{
				options = new
				{
					temperature = 0,
					top_k = 1,
					num_ctx = 1024,
					num_predict = 32,
					seed = 42,
					keep_alive = "2m"
				};
			}
			var request = new
			{
				model = model,
				prompt,
				images = new[] { base64Image },
				stream = false,
				options
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

		private bool ClipDescarta(string? response)
		{
			if (string.IsNullOrWhiteSpace(response)) return false;
			var r = response.ToLowerInvariant();
			if (Regex.IsMatch(r, "selfie|brazo extendido|m[óo]vil|tel[eé]fono")) return true;
			if (Regex.IsMatch(r, "camiseta|polo|t-?shirt|deportiva|hoodie|sudadera")) return true;
			if (Regex.IsMatch(r, "sombrero|gorra|gafas de sol|mascarilla|aud[ií]fonos")) return true;
			if (Regex.IsMatch(r, "varias personas|grupo|m[uú]ltiples")) return true;
			if (Regex.IsMatch(r, @"fondo\s+(?!blanco)[a-záéíóú]+|fondo\s+no\s+blanco")) return true;
			return false;
		}

		private bool ValidateResponse(string? response)
		{
			if (string.IsNullOrWhiteSpace(response))
				return false;
			var clean = response.ToLowerInvariant().Trim();

			// Negativos explícitos primero
			if (Regex.IsMatch(clean, @"\bno\s+(cumple|es\s+valida|es\s+válida)")) return false;
			if (Regex.IsMatch(clean, @"inválid|invalida")) return false;

			// Positivos explícitos
			if (Regex.IsMatch(clean, @"^(sí|si)\b")) return true;
			if (clean.Contains("cumple con todos los requisitos") || clean.Contains("cumple todos los requisitos")) return true;
			if (Regex.IsMatch(clean, @"\bes\s+val[ií]da\b")) return true;

			// Fallback: si aparece 'sí/si' en cualquier parte y no hay negativos contundentes
			if (Regex.IsMatch(clean, @"\b(sí|si)\b")) return true;

			return false;
		}
		private (bool valido, string motivo) EvaluateResponse(string? response)
		{
			if (string.IsNullOrWhiteSpace(response))
				return (false, "Respuesta vacía del modelo");

			var clean = response.ToLowerInvariant().Trim();

			// Motivos comunes de rechazo
			if (Regex.IsMatch(clean, @"fondo no blanco|fondo.*(no)\s+blanco|texturas|objetos|editados"))
				return (false, "Fondo no blanco o con elementos");
			if (Regex.IsMatch(clean, "ropa informal|camiseta|deportiva|estampados"))
				return (false, "Vestimenta no formal");
			if (Regex.IsMatch(clean, "selfie|inclinaci[oó]n|pose"))
				return (false, "Postura/ángulo tipo selfie o pose");
			if (Regex.IsMatch(clean, "obstrucciones|gafas de sol|mascarillas|sombreros|aud[ií]fonos"))
				return (false, "Rostro obstruido o accesorios no permitidos");
			if (Regex.IsMatch(clean, "desenfoque|baja iluminaci[oó]n|ruido|filtros"))
				return (false, "Calidad de imagen insuficiente");

			// Si pasa reglas de rechazo, usar el booleano general
			var ok = ValidateResponse(response);
			return ok ? (true, "") : (false, "El modelo no confirmó explícitamente que sea válida");
		}
		private string BuildMaleValidationPrompt()
		{
			return @"Evalúa esta imagen como VALIDA para matrícula profesional SOLO si cumple TODOS estos requisitos (responde EXCLUSIVAMENTE 'sí' o 'no'):

1) FONDO: Blanco uniforme (tipo pasaporte/carnet). Sin texturas, sin objetos, sin colores añadidos, sin recortes ni fondos editados. Se tolera SOMBRA MUY SUAVE.

2) VESTIMENTA (HOMBRE): Saco/terno o chaqueta formal y camisa de vestir. Corbata preferible. NO polos/camisetas, ropa deportiva, capuchas, gorros ni estampados llamativos.

3) POSTURA Y EXPRESIÓN: Rostro frontal, cabeza recta, hombros nivelados, mirada al frente, boca cerrada, expresión neutra (sin gestos de posar). NO selfies (brazo extendido) ni inclinación evidente.

4) ROSTRO Y ACCESORIOS: Cara completamente visible, sin obstrucciones. Lentes permitidos SOLO si no reflejan luz. Sin gafas de sol, sombreros, audífonos, mascarillas. Cabello no debe cubrir ojos/rostro.

5) COMPOSICIÓN Y CALIDAD: Una sola persona, encuadre tipo pasaporte (rostro y parte superior del torso ~70–80% del alto). Imagen nítida, bien iluminada, sin filtros ni desenfoque, color natural.

RECHAZA si detectas cualquiera de: fondo no blanco, ropa informal, pose de estudio/fashion, selfie, filtros, recorte inadecuado, ángulo lateral, baja iluminación/ruido, múltiples personas, gestos exagerados.

¿Cumple TODOS los requisitos?
Responde únicamente con 'sí' o 'no'. No incluyas ninguna explicación adicional.";
		}

		private string BuildFemaleValidationPrompt()
		{
			return @"Evalúa esta imagen como VALIDA para matrícula profesional SOLO si cumple TODOS estos requisitos (responde EXCLUSIVAMENTE 'sí' o 'no'):

1) FONDO: Blanco uniforme (tipo pasaporte/carnet). Sin texturas, sin objetos, sin colores añadidos, sin recortes ni fondos editados. Se tolera SOMBRA MUY SUAVE.

2) VESTIMENTA (MUJER): Blusa y/o chaqueta formal de oficina o vestido formal sobrio. NO camisetas/tops informales, ropa deportiva ni estampados llamativos.

3) POSTURA Y EXPRESIÓN: Rostro frontal, cabeza recta, hombros nivelados, mirada al frente, boca cerrada, expresión neutra (sin gestos de posar). NO selfies (brazo extendido) ni inclinación evidente.

4) ROSTRO Y ACCESORIOS: Cara completamente visible, sin obstrucciones. Lentes permitidos SOLO si no reflejan luz. Sin gafas de sol, sombreros, audífonos, mascarillas. Cabello no debe cubrir ojos/rostro. Aretes/collar discretos aceptables.

5) COMPOSICIÓN Y CALIDAD: Una sola persona, encuadre tipo pasaporte (rostro y parte superior del torso ~70–80% del alto). Imagen nítida, bien iluminada, sin filtros ni desenfoque, color natural.

RECHAZA si detectas cualquiera de: fondo no blanco, ropa informal, pose de estudio/fashion, selfie, filtros, recorte inadecuado, ángulo lateral, baja iluminación/ruido, múltiples personas, gestos exagerados.

¿Cumple TODOS los requisitos?
Responde únicamente con 'sí' o 'no'. No incluyas ninguna explicación adicional.";
		}

		private class QuickChecks
		{
			public string Genero { get; set; } = "indeterminado";
			public bool FondoBlanco { get; set; }
			public bool PosturaFrontal { get; set; }
			public bool RostroVisible { get; set; }
			public bool VestimentaFormal { get; set; }
			public bool EsSelfie { get; set; }
			public bool? Corbata { get; set; }
		}

		private QuickChecks? ParseQuickChecks(string? response)
		{
			if (string.IsNullOrWhiteSpace(response)) return null;
			var r = response.ToLowerInvariant();
			var qc = new QuickChecks();
			qc.Genero = Regex.Match(r, @"genero:(hombre|mujer|desconocido)").Groups.Count > 1 ? Regex.Match(r, @"genero:(hombre|mujer|desconocido)").Groups[1].Value : "indeterminado";
			qc.FondoBlanco = Regex.IsMatch(r, @"fondo:blanco\b");
			qc.PosturaFrontal = Regex.IsMatch(r, @"postura:frontal\b");
			qc.RostroVisible = Regex.IsMatch(r, @"rostro:visible\b");
			qc.VestimentaFormal = Regex.IsMatch(r, @"vestimenta:formal\b");
			qc.EsSelfie = Regex.IsMatch(r, @"selfie:si\b");
			if (Regex.IsMatch(r, @"corbata:(si|no|na)"))
			{
				var v = Regex.Match(r, @"corbata:(si|no|na)").Groups[1].Value;
				qc.Corbata = v == "si" ? true : v == "no" ? false : (bool?)null;
			}
			return qc;
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
					ModeloSolicitado = LlavaModel,
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
				model = LlavaModel,
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