using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<IEnumerable<DashboardTileDto>>> GetResumen([FromQuery] string perfilId, [FromQuery] int? personaId)
        {
            try
            {
                var tiles = new List<DashboardTileDto>();

                switch (perfilId)
                {
                    case "2": // Médico / Usuario
                        if (!personaId.HasValue)
                        {
                            return BadRequest("personaId es requerido para el perfil 2");
                        }

                        var totalMisSolicitudes = await _context.Solicitudes.CountAsync(s => s.PersonaId == personaId);
                        var observadas = await _context.Solicitudes
                            .Include(s => s.EstadoSolicitud)
                            .Where(s => s.PersonaId == personaId && (s.EstadoSolicitud!.Nombre.Contains("Observ")))
                            .CountAsync();
                        var firmasDigitales = await _context.Diplomas.CountAsync(d => d.PersonaId == personaId);
                        var enProceso = await _context.Solicitudes
                            .Where(s => s.PersonaId == personaId && s.EstadoSolicitudId != 13)
                            .CountAsync();

                        tiles.Add(new DashboardTileDto("Mis Solicitudes Enviadas", totalMisSolicitudes, "primary", "ri-folder-3-line", null));
                        tiles.Add(new DashboardTileDto("Observadas", observadas, "warning", "ri-error-warning-line", null));
                        tiles.Add(new DashboardTileDto("Firmas Digitales", firmasDigitales, "success", "ri-fingerprint-line", null));
                        tiles.Add(new DashboardTileDto("Trámites en Proceso", enProceso, "info", "ri-loop-left-line", null));
                        break;

                    case "6": // Secretaría General (ejemplo)
                        var diplomasParaFirma = await _context.Solicitudes.CountAsync(s => s.EstadoSolicitudId == 6);

                        // Firmados: contar archivos firmados en carpeta configurada si existe
                        int firmados = 0;
                        try
                        {
                            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmas_digitales");
                            if (Directory.Exists(basePath))
                            {
                                firmados = Directory.EnumerateFiles(basePath, "CertificadoFirmado*.pdf").Count();
                            }
                        }
                        catch { /* ignorar */ }

                        var diplomasEmitidos = await _context.Diplomas.CountAsync();

                        tiles.Add(new DashboardTileDto("Diplomas para Firma", diplomasParaFirma, "warning", "ri-draft-line", null));
                        tiles.Add(new DashboardTileDto("Firmados", firmados, "success", "ri-verified-badge-line", null));
                        tiles.Add(new DashboardTileDto("Diplomas Emitidos", diplomasEmitidos, "info", "ri-medal-line", null));
                        break;

                    case "7": // Oficina de Matrícula (ejemplo)
                        var autorizadas = await _context.Solicitudes.CountAsync(s => s.EstadoSolicitudId == 10);
                        var colegiaturasAsignadas = await _context.Personas.CountAsync(p => !string.IsNullOrEmpty(p.NumeroColegiatura));
                        var diplomasFirmados = await _context.Diplomas.CountAsync();
                        tiles.Add(new DashboardTileDto("Solicitudes Autorizadas", autorizadas, "primary", "ri-check-double-line", null));
                        tiles.Add(new DashboardTileDto("Nros. Colegiatura Asignados", colegiaturasAsignadas, "success", "ri-number-line", null));
                        tiles.Add(new DashboardTileDto("Diplomas Emitidos", diplomasFirmados, "info", "ri-medal-line", null));
                        break;

                    case "1": // Admin
                        var usuariosCreados = await _context.Usuarios.CountAsync();
                        var perfilesCreados = await _context.Perfil.CountAsync();
                        var menusActivos = await _context.Menu.CountAsync();
                        var solicitudesTotales = await _context.Solicitudes.CountAsync();
                        tiles.Add(new DashboardTileDto("Usuarios Creados", usuariosCreados, "primary", "ri-user-add-line", null));
                        tiles.Add(new DashboardTileDto("Perfiles Creados", perfilesCreados, "info", "ri-shield-user-line", null));
                        tiles.Add(new DashboardTileDto("Menús Activos", menusActivos, "success", "ri-menu-3-line", null));
                        tiles.Add(new DashboardTileDto("Solicitudes Totales", solicitudesTotales, "dark", "ri-database-2-line", "white"));
                        break;

                    default:
                        // Genérico: Top estados
                        var topEstados = await _context.Solicitudes
                            .Include(s => s.EstadoSolicitud)
                            .GroupBy(s => s.EstadoSolicitud!.Nombre)
                            .Select(g => new { Nombre = g.Key, Cantidad = g.Count() })
                            .OrderByDescending(x => x.Cantidad)
                            .Take(4)
                            .ToListAsync();
                        foreach (var te in topEstados)
                        {
                            tiles.Add(new DashboardTileDto(te.Nombre, te.Cantidad, "primary", "ri-bar-chart-line", null));
                        }
                        break;
                }

                return Ok(tiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en Dashboard/resumen: {ex.Message}");
            }
        }
    }
}


