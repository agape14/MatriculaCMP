using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using System.ComponentModel.DataAnnotations;
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
			IFormFile? resolucionFile = null)
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
				await _context.Personas.AddAsync(persona);
				await _context.SaveChangesAsync();

				// Asociar educación con persona
				educacion.PersonaId = persona.Id;
				await _context.Educaciones.AddAsync(educacion);
				await _context.SaveChangesAsync();

				// Configurar rutas de almacenamiento
				var fotosMedicosPath = Path.Combine(_env.WebRootPath, "fotos_medicos");
				var resolucionesPath = Path.Combine(_env.WebRootPath, "resoluciones");

				// Crear directorios si no existen
				Directory.CreateDirectory(fotosMedicosPath);
				Directory.CreateDirectory(resolucionesPath);

				// Guardar foto del médico
				var fotoFileName = $"foto_{persona.Id}{Path.GetExtension(foto.FileName)}";
				var fotoPath = Path.Combine(fotosMedicosPath, fotoFileName);
				await using (var stream = new FileStream(fotoPath, FileMode.Create))
				{
					await foto.CopyToAsync(stream);
				}
				persona.FotoPath = $"/fotos_medicos/{fotoFileName}";

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
