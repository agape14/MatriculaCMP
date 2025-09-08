using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatriculaCMP.Server.Data;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("verificar")]
    public class VerificacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VerificacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("documento")]
        public async Task<IActionResult> Documento([FromQuery] string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest("Parámetro id requerido");

                int solicitudId;
                try
                {
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(id));
                    if (!int.TryParse(decoded, out solicitudId))
                        return BadRequest("Id inválido");
                }
                catch
                {
                    return BadRequest("Id inválido");
                }

                // Buscar la ruta firmada más reciente si existe
                var rutaFirmas = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmas_digitales");
                var baseName = $"documento_{solicitudId}";
                var candidatos = Directory.Exists(rutaFirmas)
                    ? Directory.GetFiles(rutaFirmas, baseName + "*.pdf")
                    : Array.Empty<string>();

                string? elegido = candidatos
                    .OrderByDescending(p => Path.GetFileNameWithoutExtension(p).Count(c => c == 'F'))
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(elegido))
                {
                    // Fallback: PDF original del diploma
                    var diploma = await _context.Diplomas.FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);
                    if (diploma?.RutaPdf is string rutaRel && !string.IsNullOrEmpty(rutaRel))
                    {
                        var rutaOriginal = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Diplomas", rutaRel);
                        if (System.IO.File.Exists(rutaOriginal))
                        {
                            var bytes = await System.IO.File.ReadAllBytesAsync(rutaOriginal);
                            return File(bytes, "application/pdf", rutaRel);
                        }
                    }
                    return NotFound("Documento no encontrado");
                }

                var fileName = Path.GetFileName(elegido);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(elegido);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al verificar documento: {ex.Message}");
            }
        }
    }
}


