using System.Data;
using System.Globalization;
using System.Net;
using System.Text.Json;
using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class ReniecController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ApplicationDbContext _db;
		private readonly ILogger<ReniecController> _logger;
		private readonly IConsultaEsMedicoService _consultaEsMedicoService;

		public ReniecController(IConfiguration config, IHttpClientFactory httpClientFactory, ApplicationDbContext db, ILogger<ReniecController> logger, IConsultaEsMedicoService consultaEsMedicoService)
		{
			_config = config;
			_httpClientFactory = httpClientFactory;
			_db = db;
			_logger = logger;
			_consultaEsMedicoService = consultaEsMedicoService;
		}

		public class ReniecResultDto
		{
			public string? Nombres { get; set; }
			public string? ApellidoPaterno { get; set; }
			public string? ApellidoMaterno { get; set; }
			public DateTime? FechaNacimiento { get; set; }
			public string? Sexo { get; set; }
			public string? EstadoCivil { get; set; }
			public string? Fuente { get; set; }
		}

		[HttpGet("consulta/{dni}")]
		public async Task<IActionResult> ConsultarPorDni(string dni, [FromQuery] int personaId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8 || !dni.All(char.IsDigit))
					return BadRequest(new { message = "DNI inválido" });

				var res = await ConsultarHistoricoAsync(dni);
				if (res == null)
				{
					res = await ConsultarApiReniecAsync(dni);
					res.Fuente = "API";
					_ = GuardarEnHistoricoSilenciosoAsync(dni, res);
				}
				else
				{
					res.Fuente = "Historico";
				}

				// Actualizar Persona en BD principal si hay datos válidos
				if (!string.IsNullOrWhiteSpace(res.Nombres) || !string.IsNullOrWhiteSpace(res.ApellidoPaterno) || !string.IsNullOrWhiteSpace(res.ApellidoMaterno)
					|| res.FechaNacimiento.HasValue || !string.IsNullOrWhiteSpace(res.Sexo))
				{
					var persona = await _db.Personas.FindAsync(personaId);
					if (persona != null)
					{
						if (!string.IsNullOrWhiteSpace(res.Nombres)) persona.Nombres = res.Nombres.Trim();
						if (!string.IsNullOrWhiteSpace(res.ApellidoPaterno)) persona.ApellidoPaterno = res.ApellidoPaterno.Trim();
						if (!string.IsNullOrWhiteSpace(res.ApellidoMaterno)) persona.ApellidoMaterno = res.ApellidoMaterno.Trim();
						if (res.FechaNacimiento.HasValue) persona.FechaNacimiento = res.FechaNacimiento;
						if (string.Equals(res.Sexo, "MASCULINO", StringComparison.OrdinalIgnoreCase)) persona.Sexo = true;
						else if (string.Equals(res.Sexo, "FEMENINO", StringComparison.OrdinalIgnoreCase)) persona.Sexo = false;
						await _db.SaveChangesAsync();
					}
				}

				return Ok(res);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogWarning(ex, "RENIEC: error de red al consultar DNI {Dni}. Posible restricción por IP (solo equipos/autorizados).", dni);
				return StatusCode((int)HttpStatusCode.BadGateway, new
				{
					message = "No se pudo conectar con el servicio RENIEC. Es posible que solo acepte peticiones desde IPs autorizadas (p. ej. servidor CMP). Prueba desde el entorno de despliegue o revisa los logs del servidor.",
					detail = ex.Message
				});
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogWarning(ex, "RENIEC: timeout o cancelación al consultar DNI {Dni}.", dni);
				return StatusCode((int)HttpStatusCode.BadGateway, new
				{
					message = "El servicio RENIEC no respondió a tiempo. Puede haber restricción por IP o red.",
					detail = "Timeout"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al consultar RENIEC para DNI {Dni}. Inner: {Inner}", dni, ex.InnerException?.Message ?? "-");
				var isReniec = ex.Message.Contains("RENIEC", StringComparison.OrdinalIgnoreCase) || ex.InnerException?.Message?.Contains("RENIEC", StringComparison.OrdinalIgnoreCase) == true;
				if (isReniec)
					return StatusCode((int)HttpStatusCode.BadGateway, new { message = "El servicio RENIEC rechazó o no completó la consulta. Revisa los logs del servidor. Si trabajas en localhost, puede que solo acepte peticiones desde IPs autorizadas.", detail = ex.Message });
				return StatusCode(500, new { message = "Error interno al consultar RENIEC", detail = ex.Message });
			}
		}

		private async Task<ReniecResultDto?> ConsultarHistoricoAsync(string dni)
		{
			try
			{
				var cs = _config.GetConnectionString("ConsultasCMPConnection");
				if (string.IsNullOrWhiteSpace(cs)) return null;

				await using var cn = new SqlConnection(cs);
				await cn.OpenAsync();
				// Se asume la existencia de estas columnas; si no, el catch devolverá null
				var cmd = cn.CreateCommand();
				cmd.CommandText = @"SELECT TOP 1 Nombres, ApellidoPaterno, ApellidoMaterno 
FROM [dbo].[ConsultaReniec] WHERE Dni = @dni";
				cmd.Parameters.Add(new SqlParameter("@dni", SqlDbType.VarChar, 16) { Value = dni });
				await using var reader = await cmd.ExecuteReaderAsync();
				if (await reader.ReadAsync())
				{
					return new ReniecResultDto
					{
						Nombres = reader.IsDBNull(0) ? null : reader.GetString(0),
						ApellidoPaterno = reader.IsDBNull(1) ? null : reader.GetString(1),
						ApellidoMaterno = reader.IsDBNull(2) ? null : reader.GetString(2),
						Fuente = "Historico"
					};
				}
				return null;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "No se pudo consultar histórico de ConsultaReniec");
				return null;
			}
		}

		private async Task GuardarEnHistoricoSilenciosoAsync(string dni, ReniecResultDto data)
		{
			try
			{
				var cs = _config.GetConnectionString("ConsultasCMPConnection");
				if (string.IsNullOrWhiteSpace(cs)) return;

				await using var cn = new SqlConnection(cs);
				await cn.OpenAsync();
				var cmd = cn.CreateCommand();
				cmd.CommandText = @"INSERT INTO [dbo].[ConsultaReniec] (Dni, Nombres, ApellidoPaterno, ApellidoMaterno, FechaConsulta, Json)
VALUES (@dni, @n, @ap, @am, SYSDATETIME(), @json)";
				cmd.Parameters.Add(new SqlParameter("@dni", SqlDbType.VarChar, 16) { Value = dni });
				cmd.Parameters.Add(new SqlParameter("@n", SqlDbType.NVarChar, 200) { Value = (object?)data.Nombres ?? DBNull.Value });
				cmd.Parameters.Add(new SqlParameter("@ap", SqlDbType.NVarChar, 200) { Value = (object?)data.ApellidoPaterno ?? DBNull.Value });
				cmd.Parameters.Add(new SqlParameter("@am", SqlDbType.NVarChar, 200) { Value = (object?)data.ApellidoMaterno ?? DBNull.Value });
				var json = JsonSerializer.Serialize(data);
				cmd.Parameters.Add(new SqlParameter("@json", SqlDbType.NVarChar, -1) { Value = json });
				await cmd.ExecuteNonQueryAsync();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "No se pudo guardar en histórico ConsultaReniec");
			}
		}

		private async Task<ReniecResultDto> ConsultarApiReniecAsync(string dni)
		{
			var section = _config.GetSection("ReniecConsulta");
			var tokenUrl = section["TokenUrl"];
			var user = section["Username"];
			var pass = section["Password"];
			var consultaUrlTemplate = section["ConsultaUrl"];
			if (string.IsNullOrWhiteSpace(tokenUrl) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(consultaUrlTemplate))
				throw new InvalidOperationException("Configuración ReniecConsulta incompleta");

			var http = _httpClientFactory.CreateClient();
			http.Timeout = TimeSpan.FromSeconds(15);

			// 1. Obtener token
			using var form = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string,string>("username", user),
				new KeyValuePair<string,string>("password", pass)
			});
			var tokenResp = await http.PostAsync(tokenUrl, form);
			var tokenJson = await tokenResp.Content.ReadAsStringAsync();
			if (!tokenResp.IsSuccessStatusCode)
			{
				_logger.LogWarning("RENIEC token: {Status} {Reason}. Body: {Body}", (int)tokenResp.StatusCode, tokenResp.ReasonPhrase, tokenJson.Length > 500 ? tokenJson[..500] + "..." : tokenJson);
				throw new InvalidOperationException($"RENIEC token respondió {(int)tokenResp.StatusCode} {tokenResp.ReasonPhrase}. {tokenJson}");
			}
			using var tokenDoc = JsonDocument.Parse(tokenJson);
			var access = tokenDoc.RootElement.TryGetProperty("access", out var accessEl) ? accessEl.GetString() : null;
			if (string.IsNullOrWhiteSpace(access)) throw new InvalidOperationException("RENIEC: No se recibió token 'access' en la respuesta.");

			// 2. Consultar datos con Bearer
			var consultaUrl = consultaUrlTemplate.Replace("{DNI}", dni, StringComparison.OrdinalIgnoreCase);
			var req = new HttpRequestMessage(HttpMethod.Get, consultaUrl);
			req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
			var resp = await http.SendAsync(req);
			var dataJson = await resp.Content.ReadAsStringAsync();
			if (!resp.IsSuccessStatusCode)
			{
				_logger.LogWarning("RENIEC consulta DNI {Dni}: {Status} {Reason}. Body: {Body}", dni, (int)resp.StatusCode, resp.ReasonPhrase, dataJson.Length > 500 ? dataJson[..500] + "..." : dataJson);
				throw new InvalidOperationException($"RENIEC consulta respondió {(int)resp.StatusCode} {resp.ReasonPhrase}. {dataJson}");
			}
			using var dataDoc = JsonDocument.Parse(dataJson);
			var dto = new ReniecResultDto();
			ExtraerDatosReniec(dataDoc.RootElement, dto);
			return dto;
		}

		/// <summary>
		/// Consulta si el DNI pertenece a un médico colegiado en el CMP.
		/// Respuesta: esMedico=true si hay datos, esMedico=false si no, error si token inválido.
		/// </summary>
		[HttpGet("consulta-es-medico/{dni}")]
		public async Task<IActionResult> ConsultaEsMedico(string dni)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8 || !dni.All(char.IsDigit))
					return BadRequest(new { esMedico = false, message = "DNI inválido" });

				var result = await _consultaEsMedicoService.ConsultarAsync(dni);
				if (result.EsMedico)
					return Ok(new { esMedico = true, message = result.Message });
				return Ok(new { esMedico = false, message = result.Message ?? "No se encontraron datos para el número de documento proporcionado." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en ConsultaEsMedico para DNI {Dni}", dni);
				return StatusCode(500, new { esMedico = false, message = ex.Message });
			}
		}

		private void ExtraerDatosReniec(JsonElement root, ReniecResultDto dto)
		{
			// Búsqueda tolerante por claves comunes (ignorar mayúsculas/minúsculas y guiones bajos)
			string? TryFind(string keyLike)
			{
				foreach (var prop in root.EnumerateObject())
				{
					var name = prop.Name.ToLowerInvariant().Replace("_", "");
					if (name.Contains(keyLike)) return prop.Value.GetString();
				}
				foreach (var prop in root.EnumerateObject())
				{
					if (prop.Value.ValueKind == JsonValueKind.Object)
					{
						var v = TryFindRecursive(prop.Value, keyLike);
						if (v != null) return v;
					}
				}
				return null;
			}

			string? TryFindRecursive(JsonElement el, string keyLike)
			{
				foreach (var prop in el.EnumerateObject())
				{
					var name = prop.Name.ToLowerInvariant().Replace("_", "");
					if (name.Contains(keyLike)) return prop.Value.GetString();
					if (prop.Value.ValueKind == JsonValueKind.Object)
					{
						var v = TryFindRecursive(prop.Value, keyLike);
						if (v != null) return v;
					}
				}
				return null;
			}

			dto.Nombres = TryFind("prenombres") ?? TryFind("nombres") ?? TryFind("nombre");
			dto.ApellidoPaterno = TryFind("primerapellido") ?? TryFind("apellidopaterno") ?? TryFind("paterno");
			dto.ApellidoMaterno = TryFind("segundoapellido") ?? TryFind("apellidomaterno") ?? TryFind("materno");
			dto.Sexo = TryFind("genero");
			dto.EstadoCivil = TryFind("estadocivil");

			var fechaStr = TryFind("fechanacimiento");
			if (!string.IsNullOrWhiteSpace(fechaStr)
				&& DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
				dto.FechaNacimiento = fecha;
		}
	}
}

