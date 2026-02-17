using System.Text.Json;

namespace MatriculaCMP.Services
{
	/// <summary>
	/// Resultado de la consulta "¿Es médico colegiado CMP?" por DNI.
	/// </summary>
	public class ConsultaEsMedicoResult
	{
		public bool EsMedico { get; set; }
		public string? Message { get; set; }
	}

	/// <summary>
	/// Servicio que consulta si un DNI corresponde a un médico colegiado en el CMP.
	/// Usa la URL configurada en ReniecConsulta:ConsultaEsMedico.
	/// </summary>
	public interface IConsultaEsMedicoService
	{
		Task<ConsultaEsMedicoResult> ConsultarAsync(string dni, CancellationToken cancellationToken = default);
	}

	public class ConsultaEsMedicoService : IConsultaEsMedicoService
	{
		private readonly IConfiguration _config;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ConsultaEsMedicoService> _logger;

		public ConsultaEsMedicoService(
			IConfiguration config,
			IHttpClientFactory httpClientFactory,
			ILogger<ConsultaEsMedicoService> logger)
		{
			_config = config;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public async Task<ConsultaEsMedicoResult> ConsultarAsync(string dni, CancellationToken cancellationToken = default)
		{
			var section = _config.GetSection("ReniecConsulta");
			var tokenUrl = section["TokenUrl"];
			var user = section["Username"];
			var pass = section["Password"];
			var consultaEsMedicoTemplate = section["ConsultaEsMedico"];

			if (string.IsNullOrWhiteSpace(tokenUrl) || string.IsNullOrWhiteSpace(user) ||
			    string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(consultaEsMedicoTemplate))
			{
				_logger.LogWarning("Configuración ReniecConsulta/ConsultaEsMedico incompleta");
				return new ConsultaEsMedicoResult { EsMedico = false, Message = "Configuración incompleta" };
			}

			var http = _httpClientFactory.CreateClient();
			http.Timeout = TimeSpan.FromSeconds(15);

			// 1. Obtener token
			using var form = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("username", user),
				new KeyValuePair<string, string>("password", pass)
			});
			var tokenResp = await http.PostAsync(tokenUrl, form, cancellationToken);
			var tokenJson = await tokenResp.Content.ReadAsStringAsync(cancellationToken);
			if (!tokenResp.IsSuccessStatusCode)
			{
				_logger.LogWarning("ConsultaEsMedico token: {Status} {Reason}", (int)tokenResp.StatusCode, tokenResp.ReasonPhrase);
				return new ConsultaEsMedicoResult { EsMedico = false, Message = "No se pudo obtener token para la consulta CMP" };
			}

			string? access = null;
			using (var tokenDoc = JsonDocument.Parse(tokenJson))
			{
				access = tokenDoc.RootElement.TryGetProperty("access", out var accessEl) ? accessEl.GetString() : null;
			}
			if (string.IsNullOrWhiteSpace(access))
				return new ConsultaEsMedicoResult { EsMedico = false, Message = "No se recibió token de acceso" };

			// 2. Consultar buscarmedicoxdocumento con Bearer
			var consultaUrl = consultaEsMedicoTemplate.Replace("{DNI}", dni, StringComparison.OrdinalIgnoreCase);
			var req = new HttpRequestMessage(HttpMethod.Get, consultaUrl);
			req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
			var resp = await http.SendAsync(req, cancellationToken);
			var dataJson = await resp.Content.ReadAsStringAsync(cancellationToken);

			// Token inválido o expirado
			try
			{
				using var errDoc = JsonDocument.Parse(dataJson);
				if (errDoc.RootElement.TryGetProperty("code", out var codeEl) && codeEl.GetString() == "token_not_valid")
					return new ConsultaEsMedicoResult { EsMedico = false, Message = "Token de consulta CMP inválido o expirado" };
			}
			catch { /* No es JSON de error de token */ }

			// Analizar JSON según formato de la API CMP
			try
			{
				using var dataDoc = JsonDocument.Parse(dataJson);
				var root = dataDoc.RootElement;
				var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
				var msg = root.TryGetProperty("message", out var m) ? m.GetString() : null;

				if (status == "error")
					return new ConsultaEsMedicoResult { EsMedico = false, Message = msg ?? "No se encontraron datos para el número de documento proporcionado." };

				if (status == "success")
				{
					var dataLen = root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array ? dataEl.GetArrayLength() : 0;
					if (dataLen > 0)
						return new ConsultaEsMedicoResult { EsMedico = true, Message = msg ?? "El documento corresponde a un médico colegiado en el CMP" };
					return new ConsultaEsMedicoResult { EsMedico = false, Message = msg ?? "No se encontraron datos para el número de documento proporcionado." };
				}
			}
			catch { }

			if (!resp.IsSuccessStatusCode)
			{
				_logger.LogWarning("ConsultaEsMedico DNI {Dni}: {Status} {Body}", dni, (int)resp.StatusCode, dataJson.Length > 300 ? dataJson[..300] + "..." : dataJson);
				return new ConsultaEsMedicoResult { EsMedico = false, Message = "Error en la consulta CMP" };
			}

			return new ConsultaEsMedicoResult { EsMedico = true, Message = "El documento corresponde a un médico colegiado en el CMP" };
		}
	}
}
