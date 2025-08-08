using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiplomaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public DiplomaController(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet("para-firma")]
        public async Task<IActionResult> ListarParaFirma()
        {
            try
            {
                var registros = await _context.Solicitudes
                    .Where(s => s.EstadoSolicitudId == 6)
                    .Include(s => s.Persona)
                    .Include(s => s.EstadoSolicitud)
                    .Join(_context.Diplomas,
                        s => s.Id,
                        d => d.SolicitudId,
                        (s, d) => new
                        {
                            SolicitudId = s.Id,
                            NumeroSolicitud = s.NumeroSolicitud,
                            FechaSolicitud = s.FechaSolicitud,
                            EstadoId = s.EstadoSolicitudId,
                            EstadoNombre = s.EstadoSolicitud.Nombre,
                            PersonaId = s.PersonaId,
                            NumeroDocumento = s.Persona.NumeroDocumento,
                            NombreCompleto = s.Persona.NombresCompletos,
                            NumeroColegiatura = s.Persona.NumeroColegiatura,
                            DiplomaFechaEmision = d.FechaEmision,
                            RutaPdf = d.RutaPdf
                        })
                    .OrderBy(r => r.NumeroSolicitud)
                    .ToListAsync();

                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al listar diplomas para firma: {ex.Message}");
            }
        }

        [HttpPost("preparar-firma/{solicitudId}")]
        public async Task<IActionResult> PrepararFirma(int solicitudId)
        {
            try
            {
                var diploma = await _context.Diplomas.FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);
                if (diploma == null)
                {
                    return NotFound("Diploma no encontrado");
                }

                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Diplomas", diploma.RutaPdf ?? string.Empty);
                if (!System.IO.File.Exists(sourcePath))
                {
                    return NotFound("Archivo del diploma no encontrado");
                }

                var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmas_digitales");
                Directory.CreateDirectory(targetDir);
                var targetPath = Path.Combine(targetDir, $"documento_{solicitudId}.pdf");

                System.IO.File.Copy(sourcePath, targetPath, overwrite: true);

                var publicUrl = $"/firmas_digitales/documento_{solicitudId}.pdf";
                return Ok(new { success = true, url = publicUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al preparar archivo para firma: {ex.Message}");
            }
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

        [HttpPost("generar-numero-colegiatura/{solicitudId}")]
        public async Task<IActionResult> GenerarNumeroColegiatura(int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                        .ThenInclude(p => p.Educaciones)
                            .ThenInclude(e => e.Universidad)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                // Generar número de colegiatura (por ahora en duro)
                var numeroColegiatura = $"CMP-{DateTime.Now.Year}-{solicitud.Id:D6}";

                // Actualizar el campo en la persona
                solicitud.Persona.NumeroColegiatura = numeroColegiatura;
                _context.Personas.Update(solicitud.Persona);
                await _context.SaveChangesAsync();

                return Ok(new { numeroColegiatura });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar número de colegiatura: {ex.Message}");
            }
        }

        [HttpPost("generar-diploma/{solicitudId}")]
        public async Task<IActionResult> GenerarDiploma(int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                        .ThenInclude(p => p.Educaciones)
                            .ThenInclude(e => e.Universidad)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                if (string.IsNullOrEmpty(solicitud.Persona.NumeroColegiatura))
                    return BadRequest("Debe generar primero el número de colegiatura");

                // Crear el diploma
                var diploma = new Diploma
                {
                    SolicitudId = solicitudId,
                    PersonaId = solicitud.PersonaId,
                    NombreCompleto = solicitud.Persona.NombresCompletos,
                    NumeroColegiatura = solicitud.Persona.NumeroColegiatura,
                    FechaEmision = DateTime.Now,
                    UsuarioCreacion = GetUsuarioAutenticadoId(),
                    UniversidadNombre = solicitud.Persona.Educaciones.FirstOrDefault()?.Universidad?.Nombre
                };

                // Generar PDF
                var pdfBytes = _pdfService.GenerarDiplomaPdf(diploma);

                // Guardar PDF en el servidor
                var diplomasPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Diplomas");
                Directory.CreateDirectory(diplomasPath);

                var fileName = $"diploma_{solicitud.Persona.NumeroColegiatura}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(diplomasPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
                diploma.RutaPdf = fileName;

                // Guardar en base de datos
                await _context.Diplomas.AddAsync(diploma);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Diploma generado exitosamente",
                    fileName = fileName,
                    pdfBase64 = Convert.ToBase64String(pdfBytes)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar diploma: {ex.Message}");
            }
        }

        [HttpGet("diploma/{solicitudId}")]
        public async Task<IActionResult> ObtenerDiploma(int solicitudId)
        {
            try
            {
                var diploma = await _context.Diplomas
                    .Include(d => d.Persona)
                    .Include(d => d.Solicitud)
                    .FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);

                if (diploma == null)
                    return NotFound("Diploma no encontrado");

                // Leer el archivo PDF
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Diplomas", diploma.RutaPdf);
                if (!System.IO.File.Exists(filePath))
                    return NotFound("Archivo PDF no encontrado");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return Ok(new { 
                    fileName = diploma.RutaPdf,
                    pdfBase64 = Convert.ToBase64String(fileBytes)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener diploma: {ex.Message}");
            }
        }

        [HttpPost("regenerar-diploma/{solicitudId}")]
        public async Task<IActionResult> RegenerarDiploma(int solicitudId)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona)
                        .ThenInclude(p => p.Educaciones)
                            .ThenInclude(e => e.Universidad)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound("Solicitud no encontrada");

                if (string.IsNullOrEmpty(solicitud.Persona.NumeroColegiatura))
                    return BadRequest("Debe generar primero el número de colegiatura");

                // Buscar diploma existente
                var diploma = await _context.Diplomas.FirstOrDefaultAsync(d => d.SolicitudId == solicitudId);
                if (diploma == null)
                {
                    diploma = new Diploma
                    {
                        SolicitudId = solicitudId,
                        PersonaId = solicitud.PersonaId,
                        NombreCompleto = solicitud.Persona.NombresCompletos,
                        NumeroColegiatura = solicitud.Persona.NumeroColegiatura,
                        UsuarioCreacion = GetUsuarioAutenticadoId()
                    };
                    await _context.Diplomas.AddAsync(diploma);
                }

                diploma.FechaEmision = DateTime.Now;
                diploma.UniversidadNombre = solicitud.Persona.Educaciones.FirstOrDefault()?.Universidad?.Nombre;

                // Regenerar PDF
                var pdfBytes = _pdfService.GenerarDiplomaPdf(diploma);

                // Guardar PDF en el servidor (sobre escribir o crear uno nuevo)
                var diplomasPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Diplomas");
                Directory.CreateDirectory(diplomasPath);
                var fileName = $"diploma_{solicitud.Persona.NumeroColegiatura}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(diplomasPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
                diploma.RutaPdf = fileName;
                await _context.SaveChangesAsync();

                return Ok(new { fileName, pdfBase64 = Convert.ToBase64String(pdfBytes) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al regenerar diploma: {ex.Message}");
            }
        }

        [HttpGet("pdf/{fileName}")]
        public IActionResult DescargarPdf(string fileName)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Diplomas", fileName);
                if (!System.IO.File.Exists(filePath))
                    return NotFound("Archivo no encontrado");

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al descargar PDF: {ex.Message}");
            }
        }
    }
}
