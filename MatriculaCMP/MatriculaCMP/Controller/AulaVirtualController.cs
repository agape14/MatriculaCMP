using MatriculaCMP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class AulaVirtualController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public AulaVirtualController(
			ApplicationDbContext context, 
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory)
		{
			_context = context;
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
		}

		/// <summary>
		/// Autentica al usuario con el Aula Virtual y retorna token para acceso
		/// </summary>
		[HttpPost("autenticar")]
		public async Task<IActionResult> AutenticarConAulaVirtual([FromQuery] int solicitudId)
		{
			try
			{
				// Obtener datos de la solicitud y persona
				var solicitud = await _context.Solicitudes
					.Include(s => s.Persona)
					.FirstOrDefaultAsync(s => s.Id == solicitudId);

				if (solicitud == null)
					return NotFound(new { success = false, mensaje = "No se encontró la solicitud. Por favor, verifique el número de solicitud." });
				
				if (solicitud.Persona == null)
					return NotFound(new { success = false, mensaje = "No se encontraron los datos de la persona asociada a esta solicitud." });

				// Configuración del Aula Virtual
				var apiUrl = _configuration["AulaVirtual:ApiUrl"];
				var loginEndpoint = _configuration["AulaVirtual:LoginEndpoint"];
				var secretKey = _configuration["AulaVirtual:SecretKey"];
				var bearerToken = _configuration["AulaVirtual:BearerToken"];
				var frontendUrl = _configuration["AulaVirtual:FrontendUrl"];
				var codigoCurso = _configuration["AulaVirtual:CodigoCursoEtica"] ?? "EMC01";

				if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(loginEndpoint) || string.IsNullOrEmpty(secretKey))
					return StatusCode(500, new { success = false, mensaje = "Configuración del Aula Virtual incompleta" });

				// Preparar datos de autenticación
				var loginData = new
				{
					SecretKey = secretKey,
					numeroDocumento = solicitud.Persona.NumeroDocumento,
					email = solicitud.Persona.Email
				};

				// Llamar al API del Aula Virtual
				var httpClient = _httpClientFactory.CreateClient();
				
				// Agregar Bearer Token en el header Authorization
				if (!string.IsNullOrEmpty(bearerToken))
				{
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
				}
				
				var jsonContent = JsonSerializer.Serialize(loginData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var requestUrl = $"{apiUrl}{loginEndpoint}";
				var response = await httpClient.PostAsync(requestUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					var loginResponse = JsonSerializer.Deserialize<AulaVirtualLoginApiResponse>(
						responseContent, 
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

					// Éxito: status 200 y token en data (p. ej. "Usuario autenticado exitosamente." o "Usuario existente autenticado.")
					if (loginResponse?.status == 200 && !string.IsNullOrEmpty(loginResponse.data))
					{
						var token = loginResponse.data;
						var urlAulaVirtual = $"{frontendUrl}/home?token={token}&codigo={codigoCurso}";

						return Ok(new
						{
							success = true,
							token = token,
							urlAulaVirtual = urlAulaVirtual,
							mensaje = loginResponse.title ?? "Autenticación exitosa",
							apiAulaVirtual = new
							{
								url = requestUrl,
								httpStatusCode = (int)response.StatusCode,
								status = loginResponse.status,
								title = loginResponse.title,
								data = "(token)",
								body = responseContent
							}
						});
					}
					else
					{
						return BadRequest(new 
						{ 
							success = false, 
							mensaje = loginResponse?.title ?? "Error en la autenticación",
							apiAulaVirtual = new
							{
								url = requestUrl,
								httpStatusCode = (int)response.StatusCode,
								status = loginResponse?.status,
								title = loginResponse?.title,
								data = (string?)null,
								body = responseContent
							}
						});
					}
				}
				else
				{
					var mensajeError = ExtraerMensajeError(responseContent);
					return StatusCode((int)response.StatusCode, new 
					{ 
						success = false, 
						mensaje = mensajeError,
						apiAulaVirtual = new
						{
							url = requestUrl,
							httpStatusCode = (int)response.StatusCode,
							body = responseContent
						}
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, mensaje = $"Error inesperado: {ex.Message}" });
			}
		}

		/// <summary>
		/// Redirige al usuario al aula virtual externa con returnUrl (método legacy)
		/// </summary>
		[HttpGet("iniciar")]
		public IActionResult IniciarCurso([FromQuery] int solicitudId, [FromQuery] string returnUrl)
		{
			try
			{
				var aulaVirtualUrl = _configuration["AulaVirtual:FrontendUrl"] ?? "https://aulavirtual.cmp.org.pe/curso-etica";
				var returnUrlParam = _configuration["AulaVirtual:ReturnUrlParam"] ?? "returnUrl";

				// Construir URL del aula virtual con solicitudId y returnUrl
				var urlCompleta = $"{aulaVirtualUrl}?solicitudId={solicitudId}&{returnUrlParam}={Uri.EscapeDataString(returnUrl)}";

				return Redirect(urlCompleta);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Error al iniciar curso: {ex.Message}" });
			}
		}

		/// <summary>
		/// Verifica el avance del curso en el Aula Virtual externa
		/// </summary>
		[HttpGet("verificar-avance")]
		public async Task<IActionResult> VerificarAvanceCurso([FromQuery] int solicitudId)
		{
			try
			{
				// Obtener datos de la solicitud
				var solicitud = await _context.Solicitudes
					.Include(s => s.Persona)
					.Include(s => s.EstadoSolicitud)
					.FirstOrDefaultAsync(s => s.Id == solicitudId);

				if (solicitud == null)
					return NotFound(new { success = false, mensaje = "No se encontró la solicitud. Por favor, verifique el número de solicitud." });
				
				if (solicitud.Persona == null)
					return NotFound(new { success = false, mensaje = "No se encontraron los datos de la persona asociada a esta solicitud." });

				// Configuración del Aula Virtual
				var apiUrl = _configuration["AulaVirtual:ApiUrl"];
				var loginEndpoint = _configuration["AulaVirtual:LoginEndpoint"];
				var verificarEndpoint = _configuration["AulaVirtual:VerificarAvanceEndpoint"];
				var codigoCurso = _configuration["AulaVirtual:CodigoCursoEtica"] ?? "EMC01";
				var secretKey = _configuration["AulaVirtual:SecretKey"];
				var bearerToken = _configuration["AulaVirtual:BearerToken"];

				if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(loginEndpoint) || string.IsNullOrEmpty(verificarEndpoint))
					return StatusCode(500, new { success = false, mensaje = "Configuración del Aula Virtual incompleta" });

				// Primero autenticar para obtener token
				var loginData = new
				{
					SecretKey = secretKey,
					numeroDocumento = solicitud.Persona.NumeroDocumento,
					email = solicitud.Persona.Email
				};

				var httpClient = _httpClientFactory.CreateClient();
				
				// Agregar Bearer Token en el header Authorization para el login
				if (!string.IsNullOrEmpty(bearerToken))
				{
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
				}
				
				var loginContent = new StringContent(
					JsonSerializer.Serialize(loginData), 
					Encoding.UTF8, 
					"application/json");

				var loginResponse = await httpClient.PostAsync($"{apiUrl}{loginEndpoint}", loginContent);
				var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
				
				if (!loginResponse.IsSuccessStatusCode)
				{
					var mensajeError = ExtraerMensajeError(loginResponseContent);
					return BadRequest(new { 
						success = false, 
						mensaje = mensajeError
					});
				}

				var loginResult = JsonSerializer.Deserialize<AulaVirtualLoginApiResponse>(
					loginResponseContent, 
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

				// Éxito: status 200 y token en data ("Usuario autenticado exitosamente." o "Usuario existente autenticado.")
				if (loginResult?.status != 200 || string.IsNullOrEmpty(loginResult.data))
				{
					var mensajeError = loginResult?.title ?? "No se pudo autenticar con el Aula Virtual. Por favor, verifique sus datos e intente nuevamente.";
					return BadRequest(new { 
						success = false, 
						mensaje = mensajeError
					});
				}

				var token = loginResult.data;

				// Limpiar headers anteriores y agregar el token del login en Authorization Bearer
				httpClient.DefaultRequestHeaders.Remove("Authorization");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

				// Ahora verificar avance con GET usando el token en Authorization Bearer
				var avanceResponse = await httpClient.GetAsync($"{apiUrl}{verificarEndpoint}{codigoCurso}");
				var avanceContent = await avanceResponse.Content.ReadAsStringAsync();

				if (avanceResponse.IsSuccessStatusCode)
				{
					var avanceResult = JsonSerializer.Deserialize<VerificarAvanceApiResponse>(
						avanceContent, 
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

					// El API devuelve: {"status":200,"title":"Se ejecutó correctamente.","data":{"codigo":"EMC01","estadoInscripcion":"No inscrito","progresoGeneral":0,"notaFinal":0.0}}
					if (avanceResult?.status == 200 && avanceResult?.data != null)
					{
						var estadoInscripcion = avanceResult.data.estadoInscripcion ?? "";
						var progresoGeneral = avanceResult.data.progresoGeneral ?? 0;
						
						// Si el curso está aprobado, actualizar el estado de la solicitud
						if (estadoInscripcion.Equals("aprobado", StringComparison.OrdinalIgnoreCase))
						{
							if (solicitud.EstadoSolicitudId == 0) // Solo si está pendiente de curso
							{
								var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
								var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

								var estadoAnterior = solicitud.EstadoSolicitudId;
								solicitud.EstadoSolicitudId = -1; // Curso completado, pendiente de documentos
								solicitud.Observaciones = "Curso de Ética completado - Pendiente de completar documentos";

								_context.Solicitudes.Update(solicitud);

								// Registrar en historial
								var historial = new MatriculaCMP.Shared.SolicitudHistorialEstado
								{
									SolicitudId = solicitudId,
									EstadoAnteriorId = estadoAnterior,
									EstadoNuevoId = -1,
									FechaCambio = fechaCambio,
									Observacion = $"Curso de Ética aprobado - Porcentaje: {progresoGeneral}%",
									UsuarioCambio = "Sistema-AulaVirtual"
								};
								await _context.SolicitudHistorialEstados.AddAsync(historial);

								await _context.SaveChangesAsync();
							}
						}

						return Ok(new
						{
							success = true,
							estado = estadoInscripcion,
							porcentajeAvance = progresoGeneral,
							mensaje = avanceResult.title ?? "Verificación exitosa"
						});
					}
					else
					{
						return BadRequest(new 
						{ 
							success = false, 
							mensaje = avanceResult?.title ?? "No se pudo verificar el avance" 
						});
					}
				}
				else
				{
					var mensajeError = ExtraerMensajeError(avanceContent);
					return StatusCode((int)avanceResponse.StatusCode, new 
					{ 
						success = false, 
						mensaje = mensajeError
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, mensaje = $"Error inesperado al verificar el avance del curso: {ex.Message}" });
			}
		}

		/// <summary>
		/// Extrae un mensaje de error legible de la respuesta del API
		/// </summary>
		private string ExtraerMensajeError(string responseContent)
		{
			if (string.IsNullOrWhiteSpace(responseContent))
				return "No se pudo conectar con el Aula Virtual. Por favor, intente nuevamente más tarde.";

			try
			{
				// Intentar deserializar como error estándar del API
				var errorResponse = JsonSerializer.Deserialize<AulaVirtualErrorResponse>(
					responseContent,
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

				if (errorResponse != null)
				{
					// Priorizar el mensaje más descriptivo
					if (!string.IsNullOrWhiteSpace(errorResponse.message))
						return errorResponse.message;
					
					if (!string.IsNullOrWhiteSpace(errorResponse.error))
						return errorResponse.error;
					
					if (!string.IsNullOrWhiteSpace(errorResponse.title))
						return errorResponse.title;
				}

				// Si no se pudo parsear como error estándar, intentar como respuesta de login
				var loginError = JsonSerializer.Deserialize<AulaVirtualLoginApiResponse>(
					responseContent,
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

				if (loginError != null && !string.IsNullOrWhiteSpace(loginError.title))
					return loginError.title;
			}
			catch
			{
				// Si falla el parseo, devolver un mensaje genérico
			}

			// Mensaje por defecto si no se puede parsear
			return "No se pudo procesar la respuesta del Aula Virtual. Por favor, verifique sus datos e intente nuevamente.";
		}

		/// <summary>
		/// Verifica si el usuario completó el curso de ética (método legacy para compatibilidad)
		/// </summary>
		[HttpGet("estado/{solicitudId}")]
		public async Task<IActionResult> ObtenerEstadoCurso(int solicitudId)
		{
			try
			{
				var solicitud = await _context.Solicitudes
					.Include(s => s.EstadoSolicitud)
					.FirstOrDefaultAsync(s => s.Id == solicitudId);

				if (solicitud == null)
					return NotFound(new { message = "Solicitud no encontrada" });

				// Estado -1 = Curso completado, pendiente de documentos
				// Estado >= 0 pero NO -1 = Curso completado (para compatibilidad con solicitudes antiguas)
				bool completado = solicitud.EstadoSolicitudId == -1 || solicitud.EstadoSolicitudId >= 1;

				return Ok(new
				{
					solicitudId = solicitudId,
					completado = completado,
					estado = solicitud.EstadoSolicitud?.Nombre ?? "Sin estado",
					estadoId = solicitud.EstadoSolicitudId,
					mensaje = completado 
						? "El curso de ética ha sido completado" 
						: "El curso de ética está pendiente"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Error al verificar estado: {ex.Message}" });
			}
		}

		/// <summary>
		/// Marca el curso como completado (SIMULACIÓN para testing)
		/// En producción, esto lo haría el aula virtual al notificar el webhook
		/// </summary>
		[HttpPost("marcar-completado/{solicitudId}")]
		public async Task<IActionResult> MarcarCursoCompletado(int solicitudId)
		{
			try
			{
				var solicitud = await _context.Solicitudes
					.FirstOrDefaultAsync(s => s.Id == solicitudId);

				if (solicitud == null)
					return NotFound(new { message = "Solicitud no encontrada" });

				// Cambiar estado de 0 (Pendiente de curso Ética) a -1 (Curso completado, pendiente de documentos)
				if (solicitud.EstadoSolicitudId == 0)
				{
					var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
					var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

					var estadoAnterior = solicitud.EstadoSolicitudId;
					solicitud.EstadoSolicitudId = -1; // Estado intermedio: curso completado pero falta completar formulario
					solicitud.Observaciones = "Curso de Ética completado - Pendiente de completar documentos";

					_context.Solicitudes.Update(solicitud);

					// Registrar en historial
					var historial = new MatriculaCMP.Shared.SolicitudHistorialEstado
					{
						SolicitudId = solicitudId,
						EstadoAnteriorId = estadoAnterior,
						EstadoNuevoId = -1,
						FechaCambio = fechaCambio,
						Observacion = "Curso de Ética completado - Usuario puede continuar con el registro",
						UsuarioCambio = "Sistema-AulaVirtual"
					};
					await _context.SolicitudHistorialEstados.AddAsync(historial);

					await _context.SaveChangesAsync();

					return Ok(new
					{
						success = true,
						message = "Curso marcado como completado",
						solicitudId = solicitudId,
						nuevoEstado = "Curso completado, pendiente de documentos"
					});
				}

				return Ok(new
				{
					success = true,
					message = "El curso ya estaba completado",
					solicitudId = solicitudId
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
			}
		}
	}

	// Clases DTO para las respuestas del Aula Virtual API
	public class AulaVirtualLoginApiResponse
	{
		public int status { get; set; }
		public string? title { get; set; }
		public string? data { get; set; } // El token viene en data
	}

	public class VerificarAvanceApiResponse
	{
		public int status { get; set; }
		public string? title { get; set; }
		public VerificarAvanceData? data { get; set; }
	}

	public class VerificarAvanceData
	{
		public string? codigo { get; set; }
		public string? estadoInscripcion { get; set; } // "No inscrito", "En curso", "aprobado", "desaprobado"
		public int? progresoGeneral { get; set; }
		public double? notaFinal { get; set; }
	}

	public class AulaVirtualErrorResponse
	{
		public int? status { get; set; }
		public string? message { get; set; }
		public string? error { get; set; }
		public string? title { get; set; }
	}
}

