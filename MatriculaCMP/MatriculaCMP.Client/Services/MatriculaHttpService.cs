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

		public MatriculaHttpService(HttpClient http)
		{
			_http = http;
		}

		public async Task<(bool Success, string Message)> GuardarMatriculaAsync(
		Persona persona,
		Educacion educacion,
		IBrowserFile foto,
		IBrowserFile? resolucionFile = null)
		{
			try
			{
				var content = new MultipartFormDataContent();

				// Serializar los objetos a JSON (sin media type)
				var personaJson = JsonSerializer.Serialize(persona);
				var educacionJson = JsonSerializer.Serialize(educacion);

				// Añadir al formulario sin application/json
				content.Add(new StringContent(personaJson, Encoding.UTF8), "Persona");
				content.Add(new StringContent(educacionJson, Encoding.UTF8), "Educacion");

				// Foto (campo obligatorio)
				content.Add(new StreamContent(foto.OpenReadStream(foto.Size)), "Foto", foto.Name);

				// Resolución (si aplica)
				if (resolucionFile != null)
				{
					content.Add(new StreamContent(resolucionFile.OpenReadStream(resolucionFile.Size)), "ResolucionFile", resolucionFile.Name);
				}

				var response = await _http.PostAsync("api/matricula/guardar", content);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<dynamic>();
					return (true, result?.message?.ToString() ?? "Registro exitoso");
				}

				var error = await response.Content.ReadAsStringAsync();
				return (false, error);
			}
			catch (Exception ex)
			{
				return (false, $"Error: {ex.Message}");
			}
		}


	}
}
