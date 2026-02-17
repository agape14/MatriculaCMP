using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text.Json;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UniversidadController : ControllerBase
    {
        private readonly UniversidadScraper _scraper;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public UniversidadController(UniversidadScraper scraper, ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _scraper = scraper;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
			
            try
            {
                var universidades = await _context.Universidades
				.ToListAsync();

				if (universidades == null || universidades.Count == 0)
                {
                    return NotFound("No se encontraron universidades licenciadas.");
                }

                return Ok(universidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud."+ ex.Message);
            }
        }

		[HttpPost("llenar")]
		public async Task<IActionResult> LlenarTabla()
		{
			if (_context.Universidades.Any())
			{
				return Ok("La tabla de universidades ya contiene datos.");
			}

			await _scraper.ObtenerYGuardarUniversidadesAsync();
			return Ok("universidades insertados correctamente.");
		}

		[HttpGet("llenaruniversidades")]
		public async Task<IActionResult> LlenarTablaDesdeGet()
		{
			if (_context.Universidades.Any())
			{
				return Ok("La tabla de universidades ya contiene datos.");
			}

			await _scraper.ObtenerYGuardarUniversidadesAsync();
			return Ok("universidades insertados correctamente desde GET.");
		}

		[HttpGet("{paisId}")]
        public async Task<IActionResult> Get(int paisId)
        {
            var universidades = await _context.Universidades
                .Where(x => x.PaisId == paisId)
                .ToListAsync();
            return Ok(universidades);
        }

		/// <summary>
		/// Busca universidades extranjeras desde la API externa hipolabs
		/// </summary>
		/// <param name="nombrePais">Nombre del país en español o inglés</param>
		/// <returns>Lista de universidades del país</returns>
		[HttpGet("extranjeras/{nombrePais}")]
		public async Task<IActionResult> GetUniversidadesExtranjeras(string nombrePais)
		{
			try
			{
				var client = _httpClientFactory.CreateClient();
				var url = $"http://universities.hipolabs.com/search?country={Uri.EscapeDataString(nombrePais)}";
				
				var response = await client.GetAsync(url);
				
				if (response.IsSuccessStatusCode)
				{
					var json = await response.Content.ReadAsStringAsync();
					var universidades = JsonSerializer.Deserialize<List<UniversidadExtranjeraDto>>(json, new JsonSerializerOptions 
					{ 
						PropertyNameCaseInsensitive = true 
					});
					
					return Ok(universidades ?? new List<UniversidadExtranjeraDto>());
				}
				
				return Ok(new List<UniversidadExtranjeraDto>());
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error al obtener universidades extranjeras: {ex.Message}");
				return Ok(new List<UniversidadExtranjeraDto>());
			}
		}
    }

	public class UniversidadExtranjeraDto
	{
		public string name { get; set; } = string.Empty;
		public string country { get; set; } = string.Empty;
		public List<string> web_pages { get; set; } = new();
		public string alpha_two_code { get; set; } = string.Empty;
		public List<string> domains { get; set; } = new();
	}
}
