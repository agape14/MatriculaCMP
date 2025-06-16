using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class MatUbigeoController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
        public MatUbigeoController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/MatUbigeo/departamentos
		[HttpGet("departamentos")]
		public async Task<ActionResult<IEnumerable<Mat_Ubigeo>>> GetDepartamentos()
		{
			return await _context.MatUbigeos
				.Where(u => u.DepartamentoId != "00" && u.ProvinciaId == "00" && u.DistritoId == "00" && u.FlgActivo == true)
				.Select(u => new Mat_Ubigeo
				{
					DepartamentoId = u.DepartamentoId,
					Nombre = u.Nombre
				})
				.OrderBy(d => d.Nombre)
				.ToListAsync();
		}

		// GET: api/MatUbigeo/provincias/01
		[HttpGet("provincias/{departamentoId}")]
		public async Task<ActionResult<IEnumerable<Mat_Ubigeo>>> GetProvincias(string departamentoId)
		{
			return await _context.MatUbigeos
				.Where(u => u.DepartamentoId == departamentoId &&
							u.ProvinciaId != "00" &&
							u.DistritoId == "00" &&
							u.FlgActivo == true)
				.Select(u => new Mat_Ubigeo
				{
					DepartamentoId = u.DepartamentoId,
					ProvinciaId = u.ProvinciaId,
					Nombre = u.Nombre
				})
				.OrderBy(p => p.Nombre)
				.ToListAsync();
		}

		// GET: api/MatUbigeo/distritos/01/01
		[HttpGet("distritos/{departamentoId}/{provinciaId}")]
		public async Task<ActionResult<IEnumerable<Mat_Ubigeo>>> GetDistritos(string departamentoId, string provinciaId)
		{
			return await _context.MatUbigeos
				.Where(u => u.DepartamentoId == departamentoId &&
							u.ProvinciaId == provinciaId &&
							u.DistritoId != "00" &&
							u.FlgActivo == true)
				.Select(u => new Mat_Ubigeo
				{
					UbigeoId = u.UbigeoId,
					DistritoId = u.DistritoId,
					Nombre = u.Nombre
				})
				.OrderBy(d => d.Nombre)
				.ToListAsync();
		}
	}
}
