using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

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

        public async Task<(bool Success, string Message)> GuardarMatriculaAsync(
    Persona persona,
    Educacion educacion,
    IFormFile foto,
    IFormFile? resolucionFile = null,
    IDictionary<string, IFormFile?> docsPdf = null,
    int? solicitudId = null)
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

                // 1. Manejo de Persona
                var personaExistente = await _context.Personas.FindAsync(persona.Id);
                if (personaExistente != null)
                {
                    // Actualizar propiedades de persona
                    _context.Entry(personaExistente).CurrentValues.SetValues(persona);
                    persona = personaExistente;
                }
                else
                {
                    await _context.Personas.AddAsync(persona);
                }

                // 2. Manejo de Educación
                if (solicitudId.HasValue)
                {
                    var educacionExistente = await _context.Educaciones
                        .FirstOrDefaultAsync(e => e.PersonaId == persona.Id);

                    if (educacionExistente != null)
                    {
                        // Actualizar propiedades individualmente para evitar problemas con claves
                        educacionExistente.UniversidadOrigen = educacion.UniversidadOrigen;
                        educacionExistente.UniversidadId = educacion.UniversidadId;
                        educacionExistente.FechaEmisionTitulo = educacion.FechaEmisionTitulo;
                        educacionExistente.PaisUniversidadId = educacion.PaisUniversidadId;
                        educacionExistente.TipoValidacion = educacion.TipoValidacion;
                        educacionExistente.NumeroResolucion = educacion.NumeroResolucion;
                        educacionExistente.UniversidadPeruanaId = educacion.UniversidadPeruanaId;
                        educacionExistente.NombreUniversidadExtranjera = educacion.NombreUniversidadExtranjera;
                        educacionExistente.EsExtranjera = educacion.EsExtranjera;

                        educacion = educacionExistente;
                    }
                    else
                    {
                        educacion.PersonaId = persona.Id;
                        await _context.Educaciones.AddAsync(educacion);
                    }
                }
                else
                {
                    educacion.PersonaId = persona.Id;
                    await _context.Educaciones.AddAsync(educacion);
                }
                await _context.SaveChangesAsync();

                // 3. Manejo de Archivos (Foto y Resolución)
                if (foto != null && foto.Length > 0)
                {
                    var fotosMedicosPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FotosMedicos");
                    Directory.CreateDirectory(fotosMedicosPath);

                    var fotoFileName = $"foto_{persona.Id}_{Guid.NewGuid():N}{Path.GetExtension(foto.FileName)}";
                    var fotoPath = Path.Combine(fotosMedicosPath, fotoFileName);

                    await using (var stream = new FileStream(fotoPath, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                    persona.FotoPath = fotoFileName;
                }

                if (resolucionFile != null)
                {
                    var resolucionesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EducacionDocumentos");
                    Directory.CreateDirectory(resolucionesPath);

                    var resolucionFileName = $"resolucion_{persona.Id}{Path.GetExtension(resolucionFile.FileName)}";
                    var resolucionPath = Path.Combine(resolucionesPath, resolucionFileName);

                    await using (var stream = new FileStream(resolucionPath, FileMode.Create))
                    {
                        await resolucionFile.CopyToAsync(stream);
                    }
                    educacion.ResolucionPath = resolucionFileName;
                }
                await _context.SaveChangesAsync();

                // 4. Manejo de Solicitud
                Solicitud solicitud;
                if (solicitudId.HasValue)
                {
                    solicitud = await _context.Solicitudes.FindAsync(solicitudId);
                    if (solicitud == null) return (false, "Solicitud no encontrada");

                    solicitud.FechaSolicitud = fechaCambio;
                    solicitud.EstadoSolicitudId = 1;
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

                EducacionDocumento docEntity = solicitudId.HasValue
                    ? await _context.EducacionDocumentos.FirstOrDefaultAsync(d => d.EducacionId == educacion.Id)
                        ?? new EducacionDocumento { EducacionId = educacion.Id }
                    : new EducacionDocumento { EducacionId = educacion.Id };

                if (docsPdf != null)
                {
                    foreach (var kv in docsPdf.Where(kv => kv.Value != null))
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
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = solicitud.Id,
                    EstadoAnteriorId = solicitudId.HasValue ? solicitud.EstadoSolicitudId : null,
                    EstadoNuevoId = 1,
                    FechaCambio = fechaCambio,
                    Observacion = solicitudId.HasValue ? "Solicitud actualizada" : "Registro inicial de solicitud",
                    UsuarioCambio = "17"
                };
                await _context.SolicitudHistorialEstados.AddAsync(historial);
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
