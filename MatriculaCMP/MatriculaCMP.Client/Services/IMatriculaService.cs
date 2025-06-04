using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
