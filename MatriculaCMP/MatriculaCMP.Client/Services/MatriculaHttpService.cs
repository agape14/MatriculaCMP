using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

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

				content.Add(JsonContent.Create(persona), "persona");
				content.Add(JsonContent.Create(educacion), "educacion");
				content.Add(new StreamContent(foto.OpenReadStream(foto.Size)), "foto", foto.Name);

				if (resolucionFile != null)
				{
					content.Add(new StreamContent(resolucionFile.OpenReadStream(resolucionFile.Size)),
							  "resolucionFile",
							  resolucionFile.Name);
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
