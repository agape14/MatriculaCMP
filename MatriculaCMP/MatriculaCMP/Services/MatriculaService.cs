using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MatriculaCMP.Services
{
	public class MatriculaService
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _env;
        public MatriculaService(ApplicationDbContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

        // Método helper para obtener el ID del usuario autenticado
        private string GetUsuarioAutenticadoId(ClaimsPrincipal? user)
        {
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

        public async Task<(bool Success, string Message)> GuardarMatriculaAsync(
    Persona persona,
    Educacion educacion,
    IFormFile foto,
    IFormFile? resolucionFile = null,
    IDictionary<string, IFormFile?> docsPdf = null,
    int? solicitudId = null,
    ClaimsPrincipal? user = null)
        {
            // Validaciones comunes
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(educacion, new ValidationContext(educacion), validationResults, true))
            {
                return (false, string.Join(", ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Validaciones específicas para creación
            if (!solicitudId.HasValue)
            {
                if (foto == null || foto.Length == 0)
                    return (false, "Debe subir una foto");

                if (foto.Length > 25 * 1024 * 1024)
                    return (false, "La foto no debe pesar más de 25MB");

                if (Path.GetExtension(foto.FileName).ToLower() != ".jpg")
                    return (false, "La foto debe estar en formato JPG");

                if (educacion.EsExtranjera && resolucionFile == null)
                    return (false, "Debe subir el archivo de resolución para universidades extranjeras");

                var tieneSolicitudesPendientes = await _context.Solicitudes
                    .AnyAsync(s => s.PersonaId == persona.Id && s.EstadoSolicitudId != 13);

                if (tieneSolicitudesPendientes)
                    return (false, "Ya tiene una solicitud registrada en trámite. No puede registrar una nueva.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaCambio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Obtener el ID del usuario autenticado
                var usuarioId = GetUsuarioAutenticadoId(user);

                // 1. Guardar Persona (actualización selectiva para no borrar FotoPath)
                if (persona.Id == 0)
                {
                    persona.FechaRegistro = fechaCambio;
                    await _context.Personas.AddAsync(persona);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var personaDb = await _context.Personas.FirstOrDefaultAsync(p => p.Id == persona.Id);
                    if (personaDb == null)
                    {
                        return (false, "Persona no encontrada");
                    }
                    // Actualizar solo campos editables, preservar FotoPath si no se sube nueva foto
                    personaDb.ConsejoRegionalId = persona.ConsejoRegionalId;
                    personaDb.Nombres = persona.Nombres;
                    personaDb.ApellidoPaterno = persona.ApellidoPaterno;
                    personaDb.ApellidoMaterno = persona.ApellidoMaterno;
                    personaDb.Sexo = persona.Sexo;
                    personaDb.EstadoCivilId = persona.EstadoCivilId;
                    personaDb.TipoDocumentoId = persona.TipoDocumentoId;
                    personaDb.NumeroDocumento = persona.NumeroDocumento;
                    personaDb.GrupoSanguineoId = persona.GrupoSanguineoId;
                    personaDb.FechaNacimiento = persona.FechaNacimiento;
                    personaDb.PaisNacimientoId = persona.PaisNacimientoId;
                    personaDb.DepartamentoNacimientoId = persona.DepartamentoNacimientoId;
                    personaDb.ProvinciaNacimientoId = persona.ProvinciaNacimientoId;
                    personaDb.DistritoNacimientoId = persona.DistritoNacimientoId;
                    personaDb.Telefono = persona.Telefono;
                    personaDb.Celular = persona.Celular;
                    personaDb.Email = persona.Email;
                    personaDb.ZonaDomicilioId = persona.ZonaDomicilioId;
                    personaDb.DescripcionZona = persona.DescripcionZona;
                    personaDb.ViaDomicilioId = persona.ViaDomicilioId;
                    personaDb.DescripcionVia = persona.DescripcionVia;
                    personaDb.DepartamentoDomicilioId = persona.DepartamentoDomicilioId;
                    personaDb.ProvinciaDomicilioId = persona.ProvinciaDomicilioId;
                    personaDb.DistritoDomicilioId = persona.DistritoDomicilioId;
                    // personaDb.FotoPath se actualiza solo si se sube nueva foto más abajo
                    _context.Personas.Update(personaDb);
                    await _context.SaveChangesAsync();
                    // Reasignar referencia de persona para pasos siguientes
                    persona = personaDb;
                }

                // 2. Guardar Educación (buscar existente por PersonaId si está editando)
                Educacion educacionEntity;
                if (solicitudId.HasValue)
                {
                    educacionEntity = await _context.Educaciones.FirstOrDefaultAsync(e => e.PersonaId == persona.Id)
                        ?? new Educacion { PersonaId = persona.Id };
                }
                else
                {
                    educacionEntity = educacion.Id == 0
                        ? new Educacion { PersonaId = persona.Id }
                        : await _context.Educaciones.FirstOrDefaultAsync(e => e.Id == educacion.Id) ?? new Educacion { PersonaId = persona.Id };
                }

                // Actualizar campos editables
                educacionEntity.PersonaId = persona.Id;
                educacionEntity.UniversidadOrigen = educacion.UniversidadOrigen;
                educacionEntity.UniversidadId = educacion.UniversidadId;
                educacionEntity.FechaEmisionTitulo = educacion.FechaEmisionTitulo;
                educacionEntity.PaisUniversidadId = educacion.PaisUniversidadId;
                educacionEntity.TipoValidacion = educacion.TipoValidacion;
                educacionEntity.NumeroResolucion = educacion.NumeroResolucion;
                educacionEntity.UniversidadPeruanaId = educacion.UniversidadPeruanaId;
                educacionEntity.NombreUniversidadExtranjera = educacion.NombreUniversidadExtranjera;

                if (educacionEntity.Id == 0)
                {
                    await _context.Educaciones.AddAsync(educacionEntity);
                }
                else
                {
                    _context.Educaciones.Update(educacionEntity);
                }
                await _context.SaveChangesAsync();

                // 3. Manejo de Foto
                if (foto != null && foto.Length > 0)
                {
                    var fotosPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FotosMedicos");
                    Directory.CreateDirectory(fotosPath);

                    var uniqueFileName = $"{persona.Id}_{Guid.NewGuid():N}{Path.GetExtension(foto.FileName)}";
                    var savePath = Path.Combine(fotosPath, uniqueFileName);

                    await using var stream = new FileStream(savePath, FileMode.Create);
                    await foto.CopyToAsync(stream);

                    persona.FotoPath = uniqueFileName;
                    _context.Personas.Update(persona);
                    await _context.SaveChangesAsync();
                }

                // 4. Manejo de Solicitud
                Solicitud solicitud;
                if (solicitudId.HasValue)
                {
                    solicitud = await _context.Solicitudes.FindAsync(solicitudId);
                    if (solicitud == null) return (false, "Solicitud no encontrada");

                    solicitud.FechaSolicitud = fechaCambio;
                    // NO cambiar el estado - mantener el estado actual para correcciones
                    // solicitud.EstadoSolicitudId = 1; // Comentado para correcciones
                    _context.Solicitudes.Update(solicitud);
                }
                else
                {
                    var correlativo = await _context.Correlativos.FirstOrDefaultAsync();
                    if (correlativo == null)
                    {
                        correlativo = new Correlativos { UltimoNumero = 1 };
                        await _context.Correlativos.AddAsync(correlativo);
                    }
                    else
                    {
                        correlativo.UltimoNumero++;
                        _context.Correlativos.Update(correlativo);
                    }
                    await _context.SaveChangesAsync();

                    solicitud = new Solicitud
                    {
                        PersonaId = persona.Id,
                        TipoSolicitud = "REGISTRO",
                        EstadoSolicitudId = 1,
                        FechaSolicitud = fechaCambio,
                        AreaId = 1,
                        Observaciones = "Registro desde el formulario del médico",
                        NumeroSolicitud = correlativo.UltimoNumero
                    };
                    await _context.Solicitudes.AddAsync(solicitud);
                }
                await _context.SaveChangesAsync();

                // 5. Manejo de Documentos
                var eucacionDocumentossPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EducacionDocumentos");
                Directory.CreateDirectory(eucacionDocumentossPath);

                var docEntity = await _context.EducacionDocumentos.FirstOrDefaultAsync(d => d.EducacionId == educacionEntity.Id)
                                 ?? new EducacionDocumento { EducacionId = educacionEntity.Id };

                if (docsPdf != null)
                {
                    foreach (var kv in docsPdf.Where(kv => kv.Value != null && kv.Value.Length > 0))
                    {
                        var unique = $"{kv.Key}_{persona.Id}_{Guid.NewGuid():N}{Path.GetExtension(kv.Value.FileName)}";
                        var savePath = Path.Combine(eucacionDocumentossPath, unique);

                        await using var stream = new FileStream(savePath, FileMode.Create);
                        await kv.Value.CopyToAsync(stream);

                        typeof(EducacionDocumento)
                            .GetProperty($"{kv.Key}Path")?
                            .SetValue(docEntity, unique);
                    }
                }

                if (docEntity.Id > 0)
                    _context.EducacionDocumentos.Update(docEntity);
                else
                    await _context.EducacionDocumentos.AddAsync(docEntity);

                await _context.SaveChangesAsync();

                // 6. Historial de cambios
                if (!solicitudId.HasValue) // Solo para nuevas solicitudes
                {
                    var historial = new SolicitudHistorialEstado
                    {
                        SolicitudId = solicitud.Id,
                        EstadoAnteriorId = null,
                        EstadoNuevoId = 1,
                        FechaCambio = fechaCambio,
                        Observacion = "Registro inicial de solicitud",
                        UsuarioCambio = usuarioId // Usar el ID del usuario autenticado
                    };
                    await _context.SolicitudHistorialEstados.AddAsync(historial);
                }
                // Para correcciones, NO se registra cambio de estado - se mantiene el estado actual
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, solicitudId.HasValue
                    ? "Solicitud actualizada exitosamente"
                    : "Matrícula registrada exitosamente");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                return (false, $"Error de base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error inesperado: {ex.Message}");
            }
        }
    }
}
