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

        public FotoValidatorController(
            IHttpClientFactory httpClientFactory,
            ILogger<FotoValidatorController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpPost("validar")]
        [RequestSizeLimit(5_000_000)] // Limita el tamaño de la imagen a ~5MB
        public async Task<IActionResult> ValidarFoto([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Intento de validación con archivo vacío");
                    return BadRequest("Debe proporcionar un archivo de imagen válido");
                }

                // Validar tipo de archivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning($"Tipo de archivo no permitido: {fileExtension}");
                    return BadRequest($"Solo se permiten imágenes en formato {string.Join(", ", allowedExtensions)}");
                }

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                // Validar tamaño de imagen (máximo 4MB)
                if (imageBytes.Length > 4_000_000)
                {
                    _logger.LogWarning($"Imagen demasiado grande: {imageBytes.Length} bytes");
                    return BadRequest("La imagen no debe exceder los 4MB");
                }

                var base64Image = Convert.ToBase64String(imageBytes);

                // 1. Determinar el género con mayor precisión
                var generoPrompt = "Analiza la imagen y responde únicamente con 'hombre', 'mujer' o 'indeterminado'. " +
                                 "¿La imagen muestra claramente a un hombre o una mujer?";

                var generoResponse = await EnviarPreguntaALlava(base64Image, generoPrompt);
                var genero = ParseGeneroResponse(generoResponse);

                if (genero == "indeterminado")
                {
                    _logger.LogInformation("No se pudo determinar el género en la imagen");
                    return Ok(new
                    {
                        valido = false,
                        mensaje = "No se pudo determinar claramente el género en la imagen"
                    });
                }

                // 2. Validar la vestimenta y fondo según el género
                string validacionPrompt = genero == "hombre"
                    ? "Responde únicamente 'sí' o 'no'. ¿La imagen muestra a un hombre con las siguientes características: " +
                      "1. Fondo blanco, 2. Vistiendo terno formal y corbata?"
                    : "Responde únicamente 'sí' o 'no'. ¿La imagen muestra a una mujer con las siguientes características: " +
                      "1. Fondo blanco, 2. Vistiendo blusa formal o traje de oficina?";

                var validacionResponse = await EnviarPreguntaALlava(base64Image, validacionPrompt);
                var esValido = validacionResponse?.ToLowerInvariant().Contains("sí") ?? false;

                _logger.LogInformation($"Validación completada - Género: {genero}, Válido: {esValido}");

                return Ok(new
                {
                    genero,
                    valido = esValido,
                    mensaje = esValido
                        ? "Imagen válida cumple con todos los requisitos"
                        : "La imagen no cumple con los requisitos de fondo, vestimenta o visibilidad del rostro"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar la imagen");
                return StatusCode(500, "Ocurrió un error interno al procesar la imagen");
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            try
            {
                var response = await _httpClient.GetAsync(OllamaBaseUrl.Replace("/generate", "/tags"));
                return response.IsSuccessStatusCode
                    ? Ok("API FotoValidator disponible y conectada a Ollama")
                    : StatusCode(503, "API FotoValidator disponible pero Ollama no responde");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar estado de Ollama");
                return StatusCode(503, "API FotoValidator disponible pero no se pudo conectar a Ollama");
            }
        }

        private async Task<string?> EnviarPreguntaALlava(string base64Image, string prompt)
        {
            try
            {
                var request = new
                {
                    model = ModelName,
                    prompt,
                    images = new[] { base64Image },
                    stream = false,
                    options = new { temperature = 0.2 } // Reduce la creatividad para respuestas más precisas
                };

                var requestContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(OllamaBaseUrl, requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error en la API Ollama: {response.StatusCode}");
                    return null;
                }

                var result = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(result);
                return doc.RootElement.TryGetProperty("response", out var res)
                    ? res.GetString()?.Trim()
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al comunicarse con Ollama");
                return null;
            }
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
    }
}