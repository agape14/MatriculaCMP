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
    public class PersonasEducacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public PersonasEducacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método helper para obtener el ID del usuario autenticado
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
            return "Sistema"; // Fallback si no se puede obtener el ID del usuario
        }

        [HttpGet("con-educacion")]
        public async Task<ActionResult<IEnumerable<PersonaConEducacionDto>>> GetPersonasConEducacion()
        {
            var personas = await _context.Personas
                .Include(p => p.Educaciones) // Incluye las educaciones relacionadas
                .Select(p => new PersonaConEducacionDto
                {
                    PersonaId = p.Id,
                    NombresCompletos = $"{p.Nombres} {p.ApellidoPaterno}",
                    Documento = p.NumeroDocumento,
                    Educaciones = p.Educaciones.Select(e => new EducacionDto
                    {
                        UniversidadId = e.UniversidadId,
                        FechaEmision = e.FechaEmisionTitulo
                    }).ToList()
                })
                .ToListAsync();

            return Ok(personas);
        }

        // GET: api/personaseducacion/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Persona>> GetPersona(int id)
        {
            try
            {
                var persona = await _context.Personas
                    .Include(p => p.Educaciones)
                    .Include(p => p.Usuarios)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (persona == null)
                    return NotFound(new { message = $"No se encontró la persona con ID {id}" });

                return Ok(persona);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpGet("mis-solicitudes/{personaId:int}")]
        public async Task<IActionResult> ObtenerSolicitudesUsuario(int personaId)
        {
            try
            {
                var solicitudes = await _context.Solicitudes
                    .Include(s => s.Persona)
                        .ThenInclude(p => p.Educaciones)
                            .ThenInclude(e => e.Universidad)
                    .Include(s => s.Area)
                    .Include(s => s.EstadoSolicitud)
                    .Where(s => s.PersonaId == personaId)
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToListAsync();

                var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
                {
                    Id = s.Id,
                    NumeroSolicitud = s.NumeroSolicitud,
                    TipoSolicitud = s.TipoSolicitud,
                    FechaSolicitud = s.FechaSolicitud,
                    Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                    EstadoId = s.EstadoSolicitud?.Id ?? 0,
                    EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                    AreaNombre = s.Area?.Nombre ?? "No asignado",
                    PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                    UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener solicitudes: {ex.Message}" });
            }
        }

        [HttpGet("solicituddet/{id}")]
        public async Task<ActionResult<Solicitud>> GetDetalleSolicitud(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.GrupoSanguineo)
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Documento)
                .Include(s => s.EstadoSolicitud)
                .Include(s => s.HistorialEstados)
                    .ThenInclude(h => h.EstadoAnterior)
                .Include(s => s.HistorialEstados)
                    .ThenInclude(h => h.EstadoNuevo)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound("Solicitud no encontrada");

            return Ok(solicitud);
        }

        [HttpGet("fotos-medicos/{fileName}")]
        public IActionResult GetFotoMedico(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FotosMedicos", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Archivo no encontrado");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "image/jpeg");
        }

        [HttpGet("documentos-educacion/{fileName}")]
        public IActionResult GetDocumentosEducacion(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EducacionDocumentos", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Archivo no encontrado");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf");
        }

        [HttpGet("solicitudesdets")]
        public async Task<ActionResult<SolicitudSeguimientoDto>> GetDetallesSolicitudes()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                EstadoId = s.EstadoSolicitud?.Id ?? 0,
                EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                AreaNombre = s.Area?.Nombre ?? "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            return Ok(resultado);
        }

        // Solo solicitudes en estado 1 (Registrado) para Consejo Regional
        [HttpGet("solicitudes-registradas")]
        public async Task<ActionResult<IEnumerable<SolicitudSeguimientoDto>>> GetSolicitudesRegistradas()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EstadoSolicitudId == 1)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                EstadoId = s.EstadoSolicitud?.Id ?? 0,
                EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                AreaNombre = s.Area?.Nombre ?? "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("solicitudes-aprobadas-cr")]
        public async Task<ActionResult<SolicitudSeguimientoDto>> GetSolicitudesAprobadasCR()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Persona)
                    .ThenInclude(p => p.Educaciones)
                        .ThenInclude(e => e.Universidad)
                .Include(s => s.Area)
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.EstadoSolicitudId == 2) // Solo solicitudes aprobadas por Consejo Regional
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var resultado = solicitudes.Select(s => new SolicitudSeguimientoDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                TipoSolicitud = s.TipoSolicitud,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.EstadoSolicitud?.Nombre ?? "Sin Estado",
                EstadoId = s.EstadoSolicitud?.Id ?? 0,
                EstadoColor = s.EstadoSolicitud?.Color ?? "#000000",
                AreaNombre = s.Area?.Nombre ?? "No asignado",
                PersonaNombre = s.Persona?.NombresCompletos ?? "-",
                UniversidadNombre = s.Persona?.Educaciones?.FirstOrDefault()?.Universidad?.Nombre ?? "-"
            }).ToList();

            if (resultado == null || !resultado.Any())
                return NotFound(new { message = "No se encontraron solicitudes aprobadas por el Consejo Regional." });
            return Ok(resultado);
        }

        [HttpPost("cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromBody] SolicitudCambioEstadoDto dto)
        {
            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Persona) // Incluye para acceder a Email
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

                if (solicitud is null)
                    return NotFound("Solicitud no encontrada");

                var estadoAnterior = solicitud.EstadoSolicitudId;
                var nuevoEstadoId = dto.NuevoEstadoId;

                // Omitir estado 3: Si se aprueba desde Consejo Regional (estado 2), ir directo a estado 4 o 6 (Aprobado por Of. Matrícula)
                // Verificar si existe el estado 4, si no, usar el estado 6
                if (dto.NuevoEstadoId == 2 && (estadoAnterior == 1 || estadoAnterior == 0))
                {
                    var estado4Existe = await _context.EstadoSolicitudes.AnyAsync(e => e.Id == 4);
                    nuevoEstadoId = estado4Existe ? 4 : 6; // Saltar estado 3, ir directo a Aprobado por Of. Matrícula
                }

                // Cambiar estado
                solicitud.EstadoSolicitudId = nuevoEstadoId;
                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Obtener el ID del usuario autenticado
                var usuarioId = GetUsuarioAutenticadoId();

                // Guardar en historial
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = dto.SolicitudId,
                    EstadoAnteriorId = estadoAnterior,
                    EstadoNuevoId = nuevoEstadoId,
                    FechaCambio = fechaCambio,
                    Observacion = dto.Observacion,
                    UsuarioCambio = usuarioId // Usar el ID del usuario autenticado
                };

                _context.SolicitudHistorialEstados.Add(historial);
                await _context.SaveChangesAsync();

                // Enviar correo solo para estados permitidos: 1, 6, 7, 9, 11, 13
                var estadosPermitidos = new[] { 1, 6, 7, 9, 11, 13 };
                if (estadosPermitidos.Contains(nuevoEstadoId))
                {
                    var destinatario = solicitud.Persona?.Email ?? "adelacruzcarlos@gmail.com";
                    var nombre = solicitud.Persona?.Nombres ?? "Nombre";
                    var apellido = solicitud.Persona?.ApellidoPaterno ?? "Apellido";
                    await EmailHelper.EnviarCorreoCambioEstadoAsync(destinatario, nombre, apellido, nuevoEstadoId, dto.Observacion);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error al cambiar el estado: {ex.Message}");
            }
        }

        [HttpGet("EstadosConCheck/{personaId}")]
        public async Task<IActionResult> GetEstadosConCheck(int personaId, [FromQuery] bool vistaSimplificada = false)
        {
            // Obtener los estados con verReporte = true
            var estados = await _context.EstadoSolicitudes
                .Where(ed => ed.VerReporte)
                .Select(ed => new EstadoSolicitudConCheckDto
                {
                    Id = ed.Id,
                    Nombre = ed.Nombre,
                    Color = ed.Color,
                    TieneCheck = _context.Solicitudes
                        .Where(s => s.PersonaId == personaId)
                        .Join(_context.SolicitudHistorialEstados,
                            s => s.Id,
                            sd => sd.SolicitudId,
                            (s, sd) => sd)
                        .Any(sd => sd.EstadoNuevoId == ed.Id)
                })
                .ToListAsync();

            var historial = await (
                from s in _context.Solicitudes
                where s.PersonaId == personaId
                join e in _context.EstadoSolicitudes on s.EstadoSolicitudId equals e.Id
                select new
                {
                    SolicitudId = s.Id,
                    FechaSolicitud = s.FechaSolicitud,
                    EstadoId = e.Id,
                    EstadoNombre = e.Nombre,
                    EstadoColor = e.Color
                }
            ).ToListAsync();

            // Obtener diplomas para verificar fecha de entrega
            var diplomas = await _context.Diplomas
                .Where(d => _context.Solicitudes.Any(s => s.PersonaId == personaId && s.Id == d.SolicitudId))
                .Select(d => new { d.SolicitudId, d.FechaEntrega })
                .ToListAsync();

            // Si es vista simplificada (para médico), transformar estados
            if (vistaSimplificada)
            {
                var estadosSimplificados = new List<EstadoSolicitudConCheckDto>();
                
                // Mapeo de estados internos a estados visuales simplificados
                // Estado 1: Pendiente de curso Ética
                var estado1 = estados.FirstOrDefault(e => e.Id == 0 || e.Id == 1);
                if (estado1 != null)
                {
                    estadosSimplificados.Add(new EstadoSolicitudConCheckDto
                    {
                        Id = 1,
                        Nombre = "Pendiente de curso Ética",
                        Color = estado1.Color,
                        TieneCheck = estado1.TieneCheck
                    });
                }

                // Estado 2: Registrado
                var estado2 = estados.FirstOrDefault(e => e.Id == 1);
                if (estado2 != null && estado2.Id == 1)
                {
                    estadosSimplificados.Add(new EstadoSolicitudConCheckDto
                    {
                        Id = 2,
                        Nombre = "Registrado",
                        Color = estado2.Color,
                        TieneCheck = estado2.TieneCheck
                    });
                }

                // Estado 3: Aprobado por Of. Matrícula (omite estado 3 interno)
                // Verificar si pasó por estado 4 o 6 (Aprobado por Of. Matrícula)
                var estadoAprobadoOM = estados.FirstOrDefault(e => e.Id == 4 || e.Id == 6);
                if (estadoAprobadoOM != null)
                {
                    estadosSimplificados.Add(new EstadoSolicitudConCheckDto
                    {
                        Id = 3,
                        Nombre = "Aprobado por Of. Matrícula",
                        Color = estadoAprobadoOM.Color,
                        TieneCheck = estadoAprobadoOM.TieneCheck || estados.Any(e => (e.Id == 4 || e.Id == 6) && e.TieneCheck)
                    });
                }

                // Estado 4: Firmado por Consejo Regional (reemplaza estados 8 y 9 internos - Pendiente Firma Secretario CR y Decano CR)
                var estado8 = estados.FirstOrDefault(e => e.Id == 8);
                var estado9 = estados.FirstOrDefault(e => e.Id == 9);
                bool tieneFirmaCR = (estado8?.TieneCheck ?? false) || (estado9?.TieneCheck ?? false);
                estadosSimplificados.Add(new EstadoSolicitudConCheckDto
                {
                    Id = 4,
                    Nombre = "Firmado por Consejo Regional",
                    Color = estado8?.Color ?? estado9?.Color ?? "warning",
                    TieneCheck = tieneFirmaCR
                });

                // Estado 5: Firmado por Consejo Nacional (reemplaza estados 10 y 11 internos - Pendiente Firma Secretario General y Decano)
                var estado10 = estados.FirstOrDefault(e => e.Id == 10);
                var estado11 = estados.FirstOrDefault(e => e.Id == 11);
                bool tieneFirmaCN = (estado10?.TieneCheck ?? false) || (estado11?.TieneCheck ?? false);
                estadosSimplificados.Add(new EstadoSolicitudConCheckDto
                {
                    Id = 5,
                    Nombre = "Firmado por Consejo Nacional",
                    Color = estado10?.Color ?? estado11?.Color ?? "info",
                    TieneCheck = tieneFirmaCN
                });

                // Estado 6: Diploma Firmado - Entregado (reemplaza estados 12 y 13 internos)
                var estado12 = estados.FirstOrDefault(e => e.Id == 12);
                var estado13 = estados.FirstOrDefault(e => e.Id == 13);
                bool tieneDiplomaEntregado = (estado12?.TieneCheck ?? false) || (estado13?.TieneCheck ?? false);
                // Verificar si hay fecha de entrega en diplomas
                bool tieneFechaEntrega = diplomas.Any(d => d.FechaEntrega.HasValue);
                estadosSimplificados.Add(new EstadoSolicitudConCheckDto
                {
                    Id = 6,
                    Nombre = "Diploma Firmado - Entregado",
                    Color = estado13?.Color ?? "success",
                    TieneCheck = tieneDiplomaEntregado || tieneFechaEntrega
                });

                estados = estadosSimplificados;
            }

            // Calcular el porcentaje
            var totalEstados = estados.Count;
            var estadosCompletados = estados.Count(e => e.TieneCheck);
            var porcentaje = totalEstados > 0 ? (estadosCompletados * 100.0 / totalEstados) : 0;

            // Determinar último estado visual
            string nombreUltimoEstado = historial.FirstOrDefault()?.EstadoNombre ?? "Sin estado";
            if (vistaSimplificada)
            {
                // Mapear último estado interno a estado visual simplificado
                var ultimoEstadoId = historial.FirstOrDefault()?.EstadoId ?? 0;
                nombreUltimoEstado = ultimoEstadoId switch
                {
                    0 or 1 => "Registrado",
                    2 or 3 or 4 or 6 => "Aprobado por Of. Matrícula",
                    5 or 6 or 8 or 9 => "Firmado por Consejo Regional",
                    7 or 10 or 11 => "Firmado por Consejo Nacional",
                    12 or 13 => "Diploma Firmado - Entregado",
                    _ => nombreUltimoEstado
                };
                // Si hay fecha de entrega, mostrar como entregado
                if (diplomas.Any(d => d.FechaEntrega.HasValue))
                {
                    nombreUltimoEstado = "Diploma Firmado - Entregado";
                }
            }

            // Construir la respuesta
            var response = new EstadoSolicitudConCheckResponse
            {
                PorcentajeCompletado = Math.Round(porcentaje, 2), // Redondea a 2 decimales
                NombreUltimoEstado = nombreUltimoEstado,
                Estados = estados
            };

            return Ok(response);
        }

        // Serie mensual por estado basada en historial de cambios
        [HttpGet("series-estados-mes")]
        public async Task<IActionResult> GetSeriesEstadosMes([FromQuery] int? personaId, [FromQuery] int? anio)
        {
            var year = anio ?? DateTime.Now.Year;

            // Obtener estados visibles en reporte
            var estadosCatalogo = await _context.EstadoSolicitudes
                .Where(e => e.VerReporte)
                .Select(e => new { e.Id, e.Nombre })
                .ToListAsync();

            // Base query: historial por año (y persona si aplica) con join explícito a Solicitudes
            var baseQuery = from h in _context.SolicitudHistorialEstados
                            join s in _context.Solicitudes on h.SolicitudId equals s.Id
                            where h.FechaCambio.Year == year
                            select new { h.EstadoNuevoId, h.FechaCambio, s.PersonaId };

            if (personaId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.PersonaId == personaId.Value);
            }

            var datos = await baseQuery
                .GroupBy(x => new { x.EstadoNuevoId, Mes = x.FechaCambio.Month })
                .Select(g => new { EstadoId = g.Key.EstadoNuevoId, Mes = g.Key.Mes, Conteo = g.Count() })
                .ToListAsync();

            var resultado = new List<EstadoMesSerieDto>();
            foreach (var e in estadosCatalogo)
            {
                var serie = new EstadoMesSerieDto
                {
                    EstadoId = e.Id,
                    EstadoNombre = e.Nombre,
                    Meses = new int[12]
                };

                foreach (var d in datos.Where(d => d.EstadoId == e.Id))
                {
                    serie.Meses[d.Mes - 1] = d.Conteo;
                }
                resultado.Add(serie);
            }

            return Ok(resultado);
        }
    }
}
