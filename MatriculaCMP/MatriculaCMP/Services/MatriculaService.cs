using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using System.ComponentModel.DataAnnotations;

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
			if (!Validator.TryValidateObject(persona, new ValidationContext(persona), validationResults, true))
			{
				return (false, string.Join(", ", validationResults.Select(v => v.ErrorMessage)));
			}

			if (!Validator.TryValidateObject(educacion, new ValidationContext(educacion), validationResults, true))
			{
				return (false, string.Join(", ", validationResults.Select(v => v.ErrorMessage)));
			}

			// Validar foto
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

			// Validar archivo de resolución si es universidad extranjera
			if (educacion.EsExtranjera && resolucionFile == null)
			{
				return (false, "Debe subir el archivo de resolución para universidades extranjeras");
			}

			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// Guardar persona
				await _context.Personas.AddAsync(persona);
				await _context.SaveChangesAsync();

				// Asociar educación con persona
				educacion.PersonaId = persona.Id;
				await _context.Educaciones.AddAsync(educacion);
				await _context.SaveChangesAsync();

				// Guardar archivos
				var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
				if (!Directory.Exists(uploadsPath))
				{
					Directory.CreateDirectory(uploadsPath);
				}

				// Guardar foto
				var fotoFileName = $"foto_{persona.Id}{Path.GetExtension(foto.FileName)}";
				var fotoPath = Path.Combine(uploadsPath, fotoFileName);
				await using (var stream = new FileStream(fotoPath, FileMode.Create))
				{
					await foto.CopyToAsync(stream);
				}
				persona.FotoPath = $"/uploads/{fotoFileName}";

				// Guardar resolución si existe
				if (resolucionFile != null)
				{
					var resolucionFileName = $"resolucion_{persona.Id}{Path.GetExtension(resolucionFile.FileName)}";
					var resolucionPath = Path.Combine(uploadsPath, resolucionFileName);
					await using (var stream = new FileStream(resolucionPath, FileMode.Create))
					{
						await resolucionFile.CopyToAsync(stream);
					}
					educacion.ResolucionPath = $"/uploads/{resolucionFileName}";
				}

				// Actualizar paths en la base de datos
				_context.Personas.Update(persona);
				if (educacion.ResolucionPath != null)
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
				return (false, $"Error al guardar la matrícula: {ex.Message}");
			}
		}
	}
}
