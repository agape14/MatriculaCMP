using iTextSharp.text;
using iTextSharp.text.pdf;
using MatriculaCMP.Server.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace MatriculaCMP.Controller
{
	[Route("api/[controller]")]
	[ApiController]
	public class DDJJController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly ILogger<DDJJController> _logger;
		private readonly IWebHostEnvironment _env;
		private readonly ApplicationDbContext _context;

		public DDJJController(
			IConfiguration config, 
			ILogger<DDJJController> logger, 
			IWebHostEnvironment env,
			ApplicationDbContext context)
		{
			_config = config;
			_logger = logger;
			_env = env;
			_context = context;
		}
		
		// Helper para escribir logs a archivo
		private void LogToFile(string level, string message)
		{
			try
			{
				var logDir = Path.Combine(_env.ContentRootPath, "Logs");
				if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
				var logFile = Path.Combine(logDir, $"idperu-{DateTime.Now:yyyy-MM-dd}.log");
				var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
				System.IO.File.AppendAllText(logFile, logLine);
			}
			catch { /* No romper si falla */ }
		}

		// GET api/ddjj/download?nombre=...&apellidos=...&tipoDoc=...&numeroDoc=...&version=...
		[HttpGet("download")]
		public IActionResult Download([FromQuery] string nombre, [FromQuery] string apellidos, [FromQuery] string tipoDoc, [FromQuery] string numeroDoc, [FromQuery] string version = "v1.0")
		{
			using var ms = new MemoryStream();
			var doc = new Document(PageSize.A4, 50, 50, 60, 40);
			PdfWriter.GetInstance(doc, ms);
			doc.Open();

			var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
			var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

			doc.Add(new Paragraph("Declaración Jurada (DDJJ)", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 15 });
			doc.Add(new Paragraph($"Nombres y Apellidos: {nombre} {apellidos}", normalFont) { SpacingAfter = 5 });
			doc.Add(new Paragraph($"Nro. Documento: {numeroDoc}", normalFont) { SpacingAfter = 15 });

			var texto = new StringBuilder();
			texto.AppendLine("Declaro bajo juramento que la información proporcionada en el formulario de pre-matrícula es veraz, completa y corresponde a mi persona.");
			texto.AppendLine("Autorizo al Colegio Médico del Perú al tratamiento de mis datos personales conforme a la Ley N° 29733 y su Reglamento.");
			texto.AppendLine("Me comprometo a presentar documentación sustentatoria cuando sea requerida.");

			doc.Add(new Paragraph(texto.ToString(), normalFont) { SpacingAfter = 30 });
			doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont) { Alignment = Element.ALIGN_RIGHT });
			doc.Close();

			var bytes = ms.ToArray();
			Response.Headers["Content-Disposition"] = "attachment; filename=ddjj.pdf";
			return File(bytes, "application/pdf");
		}

		// GET api/ddjj/preview?nombre=...&apellidos=...&tipoDoc=...&numeroDoc=...&version=...
		[HttpGet("preview")]
		public IActionResult Preview([FromQuery] string nombre, [FromQuery] string apellidos, [FromQuery] string tipoDoc, [FromQuery] string numeroDoc, [FromQuery] string version = "v1.0", [FromQuery] bool download = false)
		{
			using var ms = new MemoryStream();
			var doc = new Document(PageSize.A4, 50, 50, 60, 40);
			PdfWriter.GetInstance(doc, ms);
			doc.Open();

			var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
			var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

			doc.Add(new Paragraph("Declaración Jurada (DDJJ)", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 15 });

			doc.Add(new Paragraph($"Nombres y Apellidos: {nombre} {apellidos}", normalFont) { SpacingAfter = 5 });
			doc.Add(new Paragraph($"Nro. Documento: {numeroDoc}", normalFont) { SpacingAfter = 15 });

			var texto = new StringBuilder();
			texto.AppendLine("Declaro bajo juramento que la información proporcionada en el formulario de pre-matrícula es veraz, completa y corresponde a mi persona.");
			texto.AppendLine("Autorizo al Colegio Médico del Perú al tratamiento de mis datos personales conforme a la Ley N° 29733 y su Reglamento.");
			texto.AppendLine("Me comprometo a presentar documentación sustentatoria cuando sea requerida.");

			doc.Add(new Paragraph(texto.ToString(), normalFont) { SpacingAfter = 30 });

			doc.Add(new Paragraph($"Fecha de visualización: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont) { Alignment = Element.ALIGN_RIGHT });
			doc.Close();

			var bytes = ms.ToArray();

			// Inline (visualización) o attachment (descarga) según parámetro
			Response.Headers["Content-Disposition"] = (download ? "attachment" : "inline") + "; filename=ddjj-preview.pdf";
			return File(bytes, "application/pdf");
		}

		// GET api/ddjj/firmar?returnUrl={url}
		[HttpGet("firmar")]
		public async Task<IActionResult> Firmar([FromQuery] string returnUrl)
		{
			var msg = $"[DDJJ] Iniciando firma ID Perú. ReturnUrl: {returnUrl}";
			_logger.LogInformation(msg);
			LogToFile("INFO", msg);
			
			// Extraer el SolicitudId del returnUrl para obtener el DNI
			var solicitudId = ExtraerSolicitudIdDeUrl(returnUrl);
			
			// Guardar el SolicitudId en una cookie para poder recuperarlo en el callback si no está en la URL
			if (solicitudId > 0)
			{
				Response.Cookies.Append("IDPeru_SolicitudId", solicitudId.ToString(), new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None, // Necesario para POST cross-site de RENIEC
					MaxAge = TimeSpan.FromMinutes(10) // Expira en 10 minutos
				});
				LogToFile("INFO", $"[DDJJ] SolicitudId guardado en cookie: {solicitudId}");
			}
			
			// Guardar el returnUrl en una cookie temporal para poder recuperarlo si hay error
			// IMPORTANTE: SameSite=None porque el callback puede venir cross-site desde RENIEC
			Response.Cookies.Append("IDPeru_ReturnUrl", returnUrl ?? "/solicitudes/prematricula", new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None, // Necesario para POST cross-site de RENIEC
				MaxAge = TimeSpan.FromMinutes(10) // Expira en 10 minutos
			});
			string? dni = null;
			
			if (solicitudId > 0)
			{
				try
				{
					// Obtener el DNI de la solicitud/persona
					var solicitud = await _context.Solicitudes
						.Include(s => s.Persona)
						.FirstOrDefaultAsync(s => s.Id == solicitudId);
					
					if (solicitud?.Persona != null)
					{
						// Solo usar DNI si el tipo de documento es DNI (62)
						if (solicitud.Persona.TipoDocumentoId == "62" && 
						    !string.IsNullOrEmpty(solicitud.Persona.NumeroDocumento))
						{
							dni = solicitud.Persona.NumeroDocumento;
							LogToFile("INFO", $"[DDJJ] DNI obtenido para SolicitudId {solicitudId}: {dni}");
						}
						else
						{
							LogToFile("WARN", $"[DDJJ] SolicitudId {solicitudId} no tiene DNI válido. TipoDoc: {solicitud.Persona.TipoDocumentoId}, NumDoc: {solicitud.Persona.NumeroDocumento}");
						}
					}
				}
				catch (Exception ex)
				{
					LogToFile("ERROR", $"[DDJJ] Error al obtener DNI: {ex.Message}");
					// Continuar sin DNI si hay error
				}
			}
			
			// Guardar el DNI en una cookie para usarlo en OnRedirectToIdentityProvider
			// Solo si es DNI válido (8 dígitos numéricos)
			if (!string.IsNullOrEmpty(dni) && dni.Length == 8 && dni.All(char.IsDigit))
			{
				Response.Cookies.Append("IDPeru_DNI", dni, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					MaxAge = TimeSpan.FromMinutes(10)
				});
				LogToFile("INFO", $"[DDJJ] DNI guardado en cookie para validación ID PERÚ: {dni}");
			}
			else
			{
				LogToFile("WARN", $"[DDJJ] DNI no válido o no encontrado. No se agregará parámetro 'vd' a la URL de ID PERÚ.");
			}
			
			// Redirige al flujo OIDC (ID PERU) usando el esquema configurado
			var redirectUri = Url.Action(nameof(Callback), "DDJJ", new { returnUrl }, Request.Scheme);
			var msg2 = $"[DDJJ] RedirectUri configurado: {redirectUri}";
			_logger.LogInformation(msg2);
			LogToFile("INFO", msg2);
			
			var props = new AuthenticationProperties
			{
				RedirectUri = redirectUri
			};
			// Pasar DNI en Properties E Items para que esté disponible en OnRedirectToIdentityProvider
			// (la cookie no está en Request hasta la siguiente petición - mismo request)
			if (!string.IsNullOrEmpty(dni) && dni.Length == 8 && dni.All(char.IsDigit))
			{
				props.Items["dni"] = dni;
				HttpContext.Items["IDPeru_DNI"] = dni; // Fallback: Items persiste en la misma petición
			}
			return Challenge(props, "IDPeru");
		}

		// GET api/ddjj/callback?returnUrl={url}
		[HttpGet("callback")]
		public async Task<IActionResult> Callback([FromQuery] string returnUrl)
		{
			var msg = $"[DDJJ] ✅ Callback recibido. ReturnUrl: {returnUrl}";
			_logger.LogInformation(msg);
			LogToFile("INFO", msg);
			
			try
			{
				var authResult = await HttpContext.AuthenticateAsync("Cookies");
				
				if (!authResult.Succeeded || authResult.Principal == null)
				{
					var errorMsg = $"[DDJJ] ❌ Autenticación fallida. Succeeded: {authResult.Succeeded}, HasPrincipal: {authResult.Principal != null}, FailureMessage: {authResult.Failure?.Message ?? "N/A"}";
					_logger.LogWarning(errorMsg);
					LogToFile("ERROR", errorMsg);
					
					// Limpiar cookies en caso de error
					Response.Cookies.Delete("IDPeru_DNI");
					Response.Cookies.Delete("IDPeru_SolicitudId");
					
					return Redirect(AgregarParametro(returnUrl, "ddjj", "error"));
				}

				// Leer claims del ciudadano de ID PERÚ
				var documentoFirmante = authResult.Principal.FindFirst("doc")?.Value;
				var firstName = authResult.Principal.FindFirst("first_name")?.Value;
				var lastName = authResult.Principal.FindFirst("last_name")?.Value;
				var email = authResult.Principal.FindFirst("email")?.Value;
				
				var successMsg = $"[DDJJ] ✅ Usuario autenticado con ID Perú. Doc: {documentoFirmante ?? "N/A"}, Nombre: {firstName ?? "N/A"}";
				_logger.LogInformation(successMsg);
				LogToFile("INFO", successMsg);

				// Obtener el DNI que se envió en el parámetro vd (de la cookie)
				string? dniEnviado = null;
				if (Request.Cookies.TryGetValue("IDPeru_DNI", out var dniCookie))
				{
					dniEnviado = dniCookie;
				}
				
				// VALIDACIÓN CRÍTICA: Verificar que el DNI validado en ID PERÚ coincida con el enviado
				if (!string.IsNullOrEmpty(dniEnviado) && !string.IsNullOrEmpty(documentoFirmante))
				{
					if (dniEnviado != documentoFirmante)
					{
						var errorValidacion = $"[DDJJ] ❌ VALIDACIÓN FALLIDA: DNI enviado ({dniEnviado}) no coincide con DNI validado ({documentoFirmante})";
						_logger.LogWarning(errorValidacion);
						LogToFile("ERROR", errorValidacion);
						
						// Limpiar cookies
						Response.Cookies.Delete("IDPeru_DNI");
						Response.Cookies.Delete("IDPeru_SolicitudId");
						
						return Redirect(AgregarParametro(returnUrl, "ddjj", "error"));
					}
					else
					{
						LogToFile("INFO", $"[DDJJ] ✅ Validación DNI exitosa: DNI enviado ({dniEnviado}) coincide con DNI validado ({documentoFirmante})");
					}
				}
				else if (!string.IsNullOrEmpty(dniEnviado))
				{
					LogToFile("WARN", $"[DDJJ] ⚠️ No se pudo validar DNI: DNI enviado ({dniEnviado}) pero no se recibió DNI en claims de ID PERÚ");
				}

				// Limpiar cookie del DNI después de usarla
				Response.Cookies.Delete("IDPeru_DNI");
				
				// Extraer el SolicitudId del returnUrl, si no está, intentar obtenerlo de la cookie
				var solicitudId = ExtraerSolicitudIdDeUrl(returnUrl);
				if (solicitudId <= 0 && Request.Cookies.TryGetValue("IDPeru_SolicitudId", out var solicitudIdCookie))
				{
					if (int.TryParse(solicitudIdCookie, out var id))
					{
						solicitudId = id;
						LogToFile("INFO", $"[DDJJ] SolicitudId obtenido de cookie: {solicitudId}");
					}
				}
				
				if (solicitudId > 0)
				{
					// Obtener la solicitud con datos de la persona
					var solicitud = await _context.Solicitudes
						.Include(s => s.Persona)
						.FirstOrDefaultAsync(s => s.Id == solicitudId);

					if (solicitud != null)
					{
						var fechaValidacion = DateTime.Now;
						var nombreCompleto = $"{solicitud.Persona?.Nombres} {solicitud.Persona?.ApellidoPaterno} {solicitud.Persona?.ApellidoMaterno}".Trim();
						var numeroDocumento = solicitud.Persona?.NumeroDocumento ?? documentoFirmante ?? "N/A";

						// Generar PDF con sello de validación
						var (pdfBytes, fileName) = GenerarDDJJConSello(
							nombreCompleto,
							numeroDocumento,
							documentoFirmante ?? numeroDocumento,
							fechaValidacion,
							solicitudId
						);

						// Guardar el PDF en disco
						var rutaRelativa = GuardarPdfEnDisco(pdfBytes, fileName);

						// Actualizar la solicitud en BD
						solicitud.DDJJFirmadaIdPeru = true;
						solicitud.FechaFirmaDDJJ = fechaValidacion;
						solicitud.DocumentoFirmanteDDJJ = documentoFirmante;
						solicitud.RutaDDJJFirmada = rutaRelativa;

						await _context.SaveChangesAsync();

						LogToFile("INFO", $"[DDJJ] ✅ DDJJ guardada. SolicitudId: {solicitudId}, Archivo: {rutaRelativa}");
					}
					else
					{
						LogToFile("WARN", $"[DDJJ] ⚠️ No se encontró solicitud con ID: {solicitudId}");
					}
				}
				else
				{
					LogToFile("WARN", $"[DDJJ] ⚠️ No se pudo extraer SolicitudId del returnUrl: {returnUrl}");
				}

				// Limpiar cookie del SolicitudId después de usarla
				Response.Cookies.Delete("IDPeru_SolicitudId");
				
				// Construir returnUrl con SolicitudId si no lo tiene
				var finalReturnUrl = returnUrl;
				if (solicitudId > 0)
				{
					// Si el returnUrl no tiene el SolicitudId, agregarlo
					if (!returnUrl.Contains($"/prematricula/{solicitudId}"))
					{
						// Si el returnUrl es solo /solicitudes/prematricula, agregar el ID
						if (returnUrl.EndsWith("/solicitudes/prematricula") || returnUrl.EndsWith("/solicitudes/prematricula/"))
						{
							finalReturnUrl = $"{returnUrl.TrimEnd('/')}/{solicitudId}";
						}
						else
						{
							// Si tiene query params, insertar el ID antes de los params
							var uri = new Uri(returnUrl);
							var path = uri.GetLeftPart(UriPartial.Path);
							if (!path.Contains($"/prematricula/{solicitudId}"))
							{
								path = path.TrimEnd('/');
								if (path.EndsWith("/prematricula"))
								{
									path = $"{path}/{solicitudId}";
								}
								finalReturnUrl = path + uri.Query;
							}
						}
					}
				}
				
				// Marca éxito para que el cliente habilite el check
				// Agregar solicitudId como parámetro para que el frontend pueda recargar el estado
				var urlConDdjj = AgregarParametro(finalReturnUrl, "ddjj", "ok");
				if (solicitudId > 0)
				{
					urlConDdjj = AgregarParametro(urlConDdjj, "solicitudId", solicitudId.ToString());
				}
				return Redirect(urlConDdjj);
			}
			catch (Exception ex)
			{
				var errorMsg = $"[DDJJ] ❌ Excepción en callback: {ex.Message}";
				_logger.LogError(ex, errorMsg);
				LogToFile("ERROR", $"{errorMsg}\n{ex.StackTrace}");
				
				// Limpiar cookies en caso de error
				Response.Cookies.Delete("IDPeru_DNI");
				Response.Cookies.Delete("IDPeru_SolicitudId");
				
				return Redirect(AgregarParametro(returnUrl, "ddjj", "error"));
			}
		}

		/// <summary>
		/// Genera el PDF de DDJJ con sello de validación ID Perú
		/// </summary>
		private (byte[] pdfBytes, string fileName) GenerarDDJJConSello(
			string nombreCompleto, 
			string numeroDocumento,
			string documentoFirmante,
			DateTime fechaValidacion,
			int solicitudId)
		{
			using var ms = new MemoryStream();
			var doc = new Document(PageSize.A4, 50, 50, 60, 40);
			var writer = PdfWriter.GetInstance(doc, ms);
			doc.Open();

			// Fuentes
			var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
			var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
			var selloTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
			var selloFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.WHITE);

			// Título
			doc.Add(new Paragraph("DECLARACIÓN JURADA (DDJJ)", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20 });

			// Datos del declarante
			doc.Add(new Paragraph($"Nombres y Apellidos: {nombreCompleto}", normalFont) { SpacingAfter = 5 });
			doc.Add(new Paragraph($"Nro. Documento: {numeroDocumento}", normalFont) { SpacingAfter = 5 });
			doc.Add(new Paragraph($"Nro. Solicitud: {solicitudId}", normalFont) { SpacingAfter = 20 });

			// Contenido de la declaración
			var texto = new StringBuilder();
			texto.AppendLine("Declaro bajo juramento que:");
			texto.AppendLine("");
			texto.AppendLine("1. La información proporcionada en el formulario de pre-matrícula es veraz, completa y corresponde a mi persona.");
			texto.AppendLine("");
			texto.AppendLine("2. Autorizo al Colegio Médico del Perú al tratamiento de mis datos personales conforme a la Ley N° 29733 - Ley de Protección de Datos Personales y su Reglamento.");
			texto.AppendLine("");
			texto.AppendLine("3. Me comprometo a presentar la documentación sustentatoria cuando sea requerida por la institución.");
			texto.AppendLine("");
			texto.AppendLine("4. Acepto que cualquier falsedad u omisión en la presente declaración es causal de nulidad del trámite y las sanciones que correspondan.");

			doc.Add(new Paragraph(texto.ToString(), normalFont) { SpacingAfter = 30 });

			// Línea separadora
			doc.Add(new Paragraph(new string('─', 80), normalFont) { SpacingAfter = 20 });

			// SELLO DE VALIDACIÓN ID PERÚ (como tabla con fondo de color)
			var selloTable = new PdfPTable(1);
			selloTable.WidthPercentage = 70;
			selloTable.HorizontalAlignment = Element.ALIGN_CENTER;

			// Celda de título del sello
			var cellTitulo = new PdfPCell(new Phrase("✓ VALIDADO CON ID PERÚ", selloTitleFont))
			{
				BackgroundColor = new BaseColor(0, 128, 0), // Verde
				HorizontalAlignment = Element.ALIGN_CENTER,
				Padding = 8,
				BorderWidth = 0
			};
			selloTable.AddCell(cellTitulo);

			// Celda de datos del sello
			var selloInfo = new StringBuilder();
			selloInfo.AppendLine($"Fecha y hora: {fechaValidacion:dd/MM/yyyy HH:mm:ss}");
			selloInfo.AppendLine($"DNI del firmante: {documentoFirmante}");
			selloInfo.AppendLine($"Método: Autenticación biométrica con DNI electrónico");

			var cellDatos = new PdfPCell(new Phrase(selloInfo.ToString(), selloFont))
			{
				BackgroundColor = new BaseColor(34, 139, 34), // Verde más oscuro
				HorizontalAlignment = Element.ALIGN_CENTER,
				Padding = 10,
				BorderWidth = 0
			};
			selloTable.AddCell(cellDatos);

			doc.Add(selloTable);

			// Pie de página
			doc.Add(new Paragraph("\n", normalFont));
			doc.Add(new Paragraph("Este documento ha sido validado electrónicamente mediante el sistema ID PERÚ de RENIEC.", 
				FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.GRAY)) 
			{ 
				Alignment = Element.ALIGN_CENTER,
				SpacingBefore = 20
			});

			doc.Close();

			var fileName = $"DDJJ_Solicitud_{solicitudId}_{fechaValidacion:yyyyMMdd_HHmmss}.pdf";
			return (ms.ToArray(), fileName);
		}

		/// <summary>
		/// Guarda el PDF en el servidor y retorna la ruta relativa
		/// </summary>
		private string GuardarPdfEnDisco(byte[] pdfBytes, string fileName)
		{
			// Crear directorio si no existe
			var ddjjPath = Path.Combine(_env.ContentRootPath, "Resources", "DDJJFirmadas");
			if (!Directory.Exists(ddjjPath))
			{
				Directory.CreateDirectory(ddjjPath);
			}

			// Guardar archivo
			var filePath = Path.Combine(ddjjPath, fileName);
			System.IO.File.WriteAllBytes(filePath, pdfBytes);

			// Retornar ruta relativa para guardar en BD
			return $"Resources/DDJJFirmadas/{fileName}";
		}

		/// <summary>
		/// Extrae el SolicitudId de la URL del returnUrl
		/// </summary>
		private int ExtraerSolicitudIdDeUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url)) return 0;

			try
			{
				// Patrones posibles: /solicitudes/prematricula/123 o /solicitudes/prematricula/123?...
				var match = Regex.Match(url, @"/prematricula/(\d+)", RegexOptions.IgnoreCase);
				if (match.Success && int.TryParse(match.Groups[1].Value, out var id))
				{
					return id;
				}
			}
			catch (Exception ex)
			{
				LogToFile("ERROR", $"Error extrayendo SolicitudId de URL: {ex.Message}");
			}

			return 0;
		}

		private static string AgregarParametro(string url, string key, string value)
		{
			if (string.IsNullOrWhiteSpace(url)) return "/";
			var separator = url.Contains("?") ? "&" : "?";
			return $"{url}{separator}{key}={value}";
		}

		/// <summary>
		/// Endpoint para descargar una DDJJ firmada
		/// </summary>
		[HttpGet("descargar-firmada/{solicitudId}")]
		public async Task<IActionResult> DescargarFirmada(int solicitudId)
		{
			try
			{
				var solicitud = await _context.Solicitudes.FindAsync(solicitudId);
				
				if (solicitud == null)
					return NotFound("Solicitud no encontrada");

				if (string.IsNullOrEmpty(solicitud.RutaDDJJFirmada))
					return NotFound("Esta solicitud no tiene DDJJ firmada");

				var filePath = Path.Combine(_env.ContentRootPath, solicitud.RutaDDJJFirmada);
				
				if (!System.IO.File.Exists(filePath))
					return NotFound("Archivo no encontrado en el servidor");

				var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
				var fileName = Path.GetFileName(filePath);

				return File(bytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				LogToFile("ERROR", $"Error al descargar DDJJ firmada: {ex.Message}");
				return StatusCode(500, "Error al descargar el archivo");
			}
		}

		/// <summary>
		/// Endpoint para verificar si una solicitud tiene DDJJ firmada y políticas aceptadas
		/// </summary>
		[HttpGet("estado/{solicitudId}")]
		public async Task<IActionResult> ObtenerEstadoDDJJ(int solicitudId)
		{
			try
			{
				var solicitud = await _context.Solicitudes
					.Where(s => s.Id == solicitudId)
					.Select(s => new 
					{
						s.DDJJFirmadaIdPeru,
						s.FechaFirmaDDJJ,
						s.DocumentoFirmanteDDJJ,
						TieneArchivo = !string.IsNullOrEmpty(s.RutaDDJJFirmada),
						s.AceptaPoliticasPrivacidad
					})
					.FirstOrDefaultAsync();

				if (solicitud == null)
					return NotFound("Solicitud no encontrada");

				return Ok(solicitud);
			}
			catch (Exception ex)
			{
				LogToFile("ERROR", $"Error al obtener estado DDJJ: {ex.Message}");
				return StatusCode(500, "Error al consultar estado");
			}
		}

		/// <summary>
		/// Endpoint para guardar la aceptación de políticas de privacidad
		/// </summary>
		[HttpPost("aceptar-politicas/{solicitudId}")]
		public async Task<IActionResult> AceptarPoliticas(int solicitudId)
		{
			try
			{
				var solicitud = await _context.Solicitudes.FindAsync(solicitudId);
				
				if (solicitud == null)
					return NotFound("Solicitud no encontrada");

				solicitud.AceptaPoliticasPrivacidad = true;
				await _context.SaveChangesAsync();

				LogToFile("INFO", $"[Políticas] Aceptadas para SolicitudId: {solicitudId}");
				return Ok(new { success = true, message = "Políticas aceptadas correctamente" });
			}
			catch (Exception ex)
			{
				LogToFile("ERROR", $"Error al guardar políticas: {ex.Message}");
				return StatusCode(500, "Error al guardar");
			}
		}
	}
}
