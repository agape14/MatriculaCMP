using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatriculaCMP.Client.Services
{
	public class MatriculaHttpService: IMatriculaService
	{
		private readonly HttpClient _http;
		private readonly ILogger<MatriculaHttpService> _logger;

		public MatriculaHttpService(HttpClient http, ILogger<MatriculaHttpService> logger)
		{
			_http = http;
			_logger = logger;
		}

		public async Task<(bool Success, string Message)> GuardarMatriculaAsync(
			Persona persona,
			Educacion educacion,
			IBrowserFile foto,
			IBrowserFile? resolucionFile = null)
		{
			try
			{
				_logger.LogInformation("Iniciando envío de matrícula para {Nombre}", persona.Nombres);

				var content = new MultipartFormDataContent();

				// Configuración de serialización
				var options = new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
				};

				// Serializar objetos
				content.Add(new StringContent(
					JsonSerializer.Serialize(persona, options),
					Encoding.UTF8,
					"application/json"),
					"Persona");

				content.Add(new StringContent(
					JsonSerializer.Serialize(educacion, options),
					Encoding.UTF8,
					"application/json"),
					"Educacion");

				// Procesar foto
				var fotoStream = foto.OpenReadStream(maxAllowedSize: 4 * 1024 * 1024); // 4MB
				content.Add(new StreamContent(fotoStream), "Foto", foto.Name);

				// Procesar resolución si existe
				if (resolucionFile != null)
				{
					var resolucionStream = resolucionFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
					content.Add(new StreamContent(resolucionStream), "ResolucionFile", resolucionFile.Name);
				}

				var response = await _http.PostAsync("api/matricula/guardar", content);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					_logger.LogError("Error al guardar matrícula: {StatusCode} - {Error}", response.StatusCode, errorContent);
					return (false, $"Error del servidor: {errorContent}");
				}

				var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
				return (true, result?.Message ?? "Matrícula registrada exitosamente");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al enviar matrícula");
				return (false, $"Error de comunicación: {ex.Message}");
			}
		}

		private class ApiResponse
		{
			public string? Message { get; set; }
		}


	}
}
