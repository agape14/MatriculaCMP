using System.Text.Json;
using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PagoController> _logger;

        public PagoController(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<PagoController> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private string GetUsuarioAutenticadoId()
        {
            var user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
            }
            return "Sistema";
        }

        [HttpPost("abrir-pasarela")]
        public async Task<IActionResult> AbrirPasarela([FromQuery] int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                if (solicitud.Persona == null)
                    return BadRequest("No se encontró información de la persona");

                // Obtener configuración de la pasarela
                var urlInicio = _configuration["PasarelaPago:UrlInicio"] ?? "https://pago.cmp.org.pe/api/seguridad/sso/init";
                var source = _configuration["PasarelaPago:Source"] ?? "publico";
                var app = _configuration["PasarelaPago:App"] ?? "4";
                var itemCode = _configuration["PasarelaPago:ItemCode"] ?? "";

                // Determinar tipo de documento
                // La pasarela espera: 0: Otros, 1: DNI, 4: Carnet extranjería, 6: RUC, 7: Pasaporte
                // TipoDocumentoId en la BD puede tener valores como "62" (DNI), "64" (Carnet extranjería), etc.
                // Necesitamos mapear el TipoDocumentoId a los valores que espera la pasarela
                var tipoDocumento = "1"; // DNI por defecto
                if (!string.IsNullOrEmpty(solicitud.Persona.TipoDocumentoId))
                {
                    // Mapear TipoDocumentoId de la BD a los valores de la pasarela
                    // "62" = DNI en BD → "1" en pasarela
                    // "64" = Carnet extranjería en BD → "4" en pasarela
                    tipoDocumento = solicitud.Persona.TipoDocumentoId switch
                    {
                        "62" => "1", // DNI
                        "64" => "4", // Carnet extranjería
                        "1" => "1",  // DNI (directo)
                        "4" => "4",  // Carnet extranjería (directo)
                        "6" => "6",  // RUC
                        "7" => "7",  // Pasaporte
                        "0" => "0",  // Otros
                        _ => "1"     // Default DNI
                    };
                }

                // La pasarela exige POST; devolvemos URL base y parámetros para enviar en el body del form
                var postParams = new Dictionary<string, string>
                {
                    ["Usuario"] = solicitud.Persona.NumeroDocumento ?? "",
                    ["TipoDocumento"] = tipoDocumento,
                    ["Source"] = source,
                    ["App"] = app
                };
                if (!string.IsNullOrEmpty(itemCode))
                    postParams["ItemCode"] = itemCode;

                return Ok(new { success = true, url = urlInicio, postParams = postParams });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al abrir pasarela: {ex.Message}");
            }
        }

        [HttpGet("verificar")]
        public async Task<IActionResult> VerificarPago([FromQuery] int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                if (solicitud.Persona == null)
                    return BadRequest("No se encontró información de la persona");

                var dni = solicitud.Persona.NumeroDocumento?.Trim();
                if (string.IsNullOrEmpty(dni))
                    return BadRequest("No se encontró número de documento (DNI) para verificar el pago.");

                var section = _configuration.GetSection("ReniecConsulta");
                var tokenUrl = section["TokenUrl"];
                var username = section["Username"];
                var password = section["Password"];
                var estadoPagoUrlTemplate = section["EstadoPagoUrl"];
                if (string.IsNullOrWhiteSpace(tokenUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(estadoPagoUrlTemplate))
                {
                    _logger.LogWarning("Configuración ReniecConsulta incompleta para verificación de pago");
                    return Ok(new { pagado = false, comprobante = "", mensaje = "No se ha configurado la verificación de pago. Contacte al administrador." });
                }

                var http = _httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(20);

                // 1. Obtener token (mismo endpoint que ReniecConsulta)
                using var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });
                var tokenResp = await http.PostAsync(tokenUrl, form);
                var tokenJson = await tokenResp.Content.ReadAsStringAsync();
                if (!tokenResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Token consultacmp: {Status} {Reason}. Body: {Body}", (int)tokenResp.StatusCode, tokenResp.ReasonPhrase, tokenJson.Length > 300 ? tokenJson[..300] + "..." : tokenJson);
                    return Ok(new { pagado = false, comprobante = "", mensaje = "No se pudo conectar con el servicio de verificación de pago. Intente más tarde." });
                }
                using (var tokenDoc = JsonDocument.Parse(tokenJson))
                {
                    var root = tokenDoc.RootElement;
                    if (!root.TryGetProperty("access", out var accessEl))
                    {
                        _logger.LogWarning("Respuesta token sin 'access'");
                        return Ok(new { pagado = false, comprobante = "", mensaje = "Error en el servicio de verificación. Intente más tarde." });
                    }
                    var access = accessEl.GetString();
                    if (string.IsNullOrWhiteSpace(access))
                        return Ok(new { pagado = false, comprobante = "", mensaje = "Error en el servicio de verificación. Intente más tarde." });

                    // 2. Consultar estado de pago por DNI
                    var estadoPagoUrl = estadoPagoUrlTemplate.Replace("{DNI}", dni, StringComparison.OrdinalIgnoreCase);
                    var req = new HttpRequestMessage(HttpMethod.Get, estadoPagoUrl);
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
                    var resp = await http.SendAsync(req);
                    var dataJson = await resp.Content.ReadAsStringAsync();
                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Estado pago DNI {Dni}: {Status} {Reason}. Body: {Body}", dni, (int)resp.StatusCode, resp.ReasonPhrase, dataJson.Length > 300 ? dataJson[..300] + "..." : dataJson);
                        return Ok(new { pagado = false, comprobante = "", mensaje = "No se pudo consultar el estado de pago. Intente más tarde." });
                    }

                    using var dataDoc = JsonDocument.Parse(dataJson);
                    var dataRoot = dataDoc.RootElement;
                    if (!dataRoot.TryGetProperty("data", out var dataArr) || dataArr.GetArrayLength() == 0)
                        return Ok(new { pagado = false, comprobante = "", mensaje = "Aún no se ha registrado el pago. Realice el pago en la pasarela antes de continuar al siguiente paso." });

                    var primerItem = dataArr[0];
                    var pagado = primerItem.TryGetProperty("Pagado", out var pagadoEl) && pagadoEl.ValueKind == JsonValueKind.True;
                    string? comprobante = null;
                    if (pagado && primerItem.TryGetProperty("NumeroCompra", out var numCompraEl))
                        comprobante = numCompraEl.GetString();

                    if (pagado)
                        return Ok(new { pagado = true, comprobante = comprobante ?? "Comprobante registrado", mensaje = "El pago ha sido verificado correctamente." });
                    return Ok(new { pagado = false, comprobante = "", mensaje = "Aún no se ha registrado el pago. Debe realizar el pago antes de continuar al siguiente paso." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar pago para solicitud {SolicitudId}", solicitudId);
                return Ok(new { pagado = false, comprobante = "", mensaje = $"Error al verificar pago: {ex.Message}" });
            }
        }

        /// <summary>
        /// [DESARROLLO] Simula pago verificado: actualiza estado a Registrado (1), registra historial y envía correo.
        /// Restringir o eliminar en producción.
        /// </summary>
        [HttpPost("marcar-pagado-desarrollo")]
        public async Task<IActionResult> MarcarPagadoDesarrollo([FromQuery] int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                var estadoAnterior = solicitud.EstadoSolicitudId;
                if (estadoAnterior == 1)
                {
                    return Ok(new { success = true, message = "La solicitud ya está en estado Registrado." });
                }

                solicitud.EstadoSolicitudId = 1; // Registrado
                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);
                var usuarioId = GetUsuarioAutenticadoId();

                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = solicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = 1,
                    FechaCambio = fechaCambio,
                    Observacion = "Pago verificado (desarrollo) - Solicitud registrada",
                    UsuarioCambio = usuarioId
                };
                _context.SolicitudHistorialEstados.Add(historial);
                await _context.SaveChangesAsync();

                var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";
                await EmailHelper.EnviarCorreoCambioEstadoAsync(
                    destinatario,
                    nombre,
                    apellido,
                    1, // Estado 1: Registrado
                    "Su solicitud ha sido registrada y está en proceso de revisión por la oficina de matrícula.");

                return Ok(new { success = true, message = "Pago simulado. Estado actualizado a Registrado y correo enviado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
