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
			IDictionary<string, IFormFile?> docsPdf = null)
		{
			// Validar campos requeridos
			var validationResults = new List<ValidationResult>();
			//if (!Validator.TryValidateObject(persona, new ValidationContext(persona), validationResults, true))
			//{
			//	return (false, string.Join(", ", validationResults.Select(v => v.ErrorMessage)));
			//}

			if (!Validator.TryValidateObject(educacion, new ValidationContext(educacion), validationResults, true))
			{
				return (false, string.Join(", ", validationResults.Select(v => v.ErrorMessage)));
			}

			// Validar foto (unificado aquí)
			if (foto == null || foto.Length == 0)
			{
				return (false, "Debe subir una foto");
			}

			if (foto.Length > 25 * 1024 * 1024) // 25MB
			{
				return (false, "La foto no debe pesar más de 25MB");
			}

			if (Path.GetExtension(foto.FileName).ToLower() != ".jpg")
			{
				return (false, "La foto debe estar en formato JPG");
			}

			// Validar archivo de resolución
			if (educacion.EsExtranjera && resolucionFile == null)
			{
				return (false, "Debe subir el archivo de resolución para universidades extranjeras");
			}

			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
                // Guardar persona para obtener ID
                //await _context.Personas.AddAsync(persona);
                //_context.Personas.Update(persona);
                //await _context.SaveChangesAsync();

				// Asociar educación con persona
				educacion.PersonaId = persona.Id;
				await _context.Educaciones.AddAsync(educacion);
				await _context.SaveChangesAsync();

				// Configurar rutas de almacenamiento
				//var fotosMedicosPath = Path.Combine(_env.WebRootPath, "fotos_medicos");
                var fotosMedicosPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FotosMedicos");
				var eucacionDocumentossPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EducacionDocumentos");
				var resolucionesPath = Path.Combine(_env.WebRootPath, "resoluciones");

				// Crear directorios si no existen
				Directory.CreateDirectory(fotosMedicosPath);
				Directory.CreateDirectory(resolucionesPath);

                // Guardar foto del médico
                var uniqueSuffix = Guid.NewGuid().ToString("N");
                var fotoFileName = $"foto_{persona.Id}_{uniqueSuffix}{Path.GetExtension(foto.FileName)}";
				var fotoPath = Path.Combine(fotosMedicosPath, fotoFileName);
				await using (var stream = new FileStream(fotoPath, FileMode.Create))
				{
					await foto.CopyToAsync(stream);
				}
				persona.FotoPath = fotoFileName;

				// Guardar resolución si existe
				if (resolucionFile != null)
				{
					var resolucionFileName = $"resolucion_{persona.Id}{Path.GetExtension(resolucionFile.FileName)}";
					var resolucionPath = Path.Combine(resolucionesPath, resolucionFileName);
					await using (var stream = new FileStream(resolucionPath, FileMode.Create))
					{
						await resolucionFile.CopyToAsync(stream);
					}
					educacion.ResolucionPath = $"/resoluciones/{resolucionFileName}";
				}

				// Actualizar entidades con rutas de archivos
				_context.Personas.Update(persona);
				if (resolucionFile != null)
				{
					_context.Educaciones.Update(educacion);
				}
				await _context.SaveChangesAsync();

                // 🔢 Obtener o crear correlativo
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

                var solicitud = new Solicitud
                {
                    PersonaId = persona.Id,
                    TipoSolicitud = "REGISTRO",
                    EstadoSolicitudId = 1, // Por ejemplo: Pendiente
                    FechaSolicitud = DateTime.Now,
                    AreaId = 1, // o algún área por defecto
                    Observaciones = "Registro desde el formulario del médico",
                    NumeroSolicitud = correlativo.UltimoNumero
                };

                await _context.Solicitudes.AddAsync(solicitud);
                await _context.SaveChangesAsync();

				//var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UsuarioId")?.Value
				//?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
				//?? "Sistema"; // Fallback si no hay usuario
				// 📋 Guardar historial de estado
                var historial = new SolicitudHistorialEstado
                {
                    SolicitudId = solicitud.Id,
                    EstadoAnteriorId = null,
                    EstadoNuevoId = solicitud.EstadoSolicitudId,
                    FechaCambio = DateTime.Now,
                    Observacion = "Registro inicial de solicitud",
                    UsuarioCambio = "17"
                };
                await _context.SolicitudHistorialEstados.AddAsync(historial);
                await _context.SaveChangesAsync();





				// Guardar documentos PDF si existen
				var docEntity = new EducacionDocumento { EducacionId = educacion.Id };

				foreach (var kv in docsPdf)
				{
					if (kv.Value == null) continue;

					var original = kv.Value;
					//var folder = Path.Combine(_env.WebRootPath, "documentos");
					Directory.CreateDirectory(eucacionDocumentossPath);

					var unique = $"{kv.Key}_{persona.Id}_{Guid.NewGuid():N}{Path.GetExtension(original.FileName)}";
					var savePath = Path.Combine(eucacionDocumentossPath, unique);

					await using var stream = new FileStream(savePath, FileMode.Create);
					await original.CopyToAsync(stream);

					// ↪  usa reflexión o switch para asignar la propiedad correcta
					typeof(EducacionDocumento)
						.GetProperty($"{kv.Key}Path")?
						.SetValue(docEntity, $"/documentos/{unique}");
				}

				await _context.EducacionDocumentos.AddAsync(docEntity);
				await _context.SaveChangesAsync();



				await transaction.CommitAsync();

				return (true, "Matrícula registrada exitosamente");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();

				var fullError = new StringBuilder();
				fullError.AppendLine(ex.Message);

				var inner = ex.InnerException;
				while (inner != null)
				{
					fullError.AppendLine(inner.Message);
					inner = inner.InnerException;
				}

				return (false, $"Error al guardar la matrícula: {fullError}");
			}
		}
	}
}
