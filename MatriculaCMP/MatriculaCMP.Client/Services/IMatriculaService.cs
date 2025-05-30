using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace MatriculaCMP.Client.Services
{
	public interface IMatriculaService
	{
		Task<(bool Success, string Message)> GuardarMatriculaAsync(
		 Persona persona,
		 Educacion educacion,
		 IBrowserFile foto,
		 IBrowserFile? resolucionFile = null);
	}
}
