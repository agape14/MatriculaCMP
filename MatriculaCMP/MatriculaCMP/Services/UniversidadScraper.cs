using HtmlAgilityPack;
using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Services
{
    public class UniversidadScraper
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
		private readonly ApplicationDbContext _context;
		public UniversidadScraper(IConfiguration config, ApplicationDbContext context)
        {
            _httpClient = new HttpClient();
            _config = config;
			_context = context;
		}

		public async Task ObtenerYGuardarUniversidadesAsync()
		{
			var lista = new List<UniversidadLicenciada>();

			// Obtener configuraciones
			var linkweb = _config["SuneduScrapping:LinkWeb"];
			var idtabla = _config["SuneduScrapping:IdTabla"];

			// 1. Obtener HTML
			var html = await _httpClient.GetStringAsync(linkweb);
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			// 2. Buscar tabla por ID
			var table = doc.DocumentNode.SelectSingleNode(idtabla);
			if (table == null) return;

			// 3. Obtener filas
			var rows = table.SelectNodes(".//tbody/tr");
			if (rows == null) return;

			// 4. Obtener nombres de universidades existentes para evitar duplicados
			var universidadesExistentes = await _context.Universidades
				.Select(u => u.Nombre)
				.ToListAsync();

			var nuevasUniversidades = new List<Universidad>();

			foreach (var row in rows)
			{
				var cols = row.SelectNodes("td");
				if (cols != null && cols.Count >= 5)
				{
					var nombre = cols[0].InnerText.Trim();

					// Verificar si ya existe
					if (!universidadesExistentes.Contains(nombre))
					{
						var paisPeru = await _context.Paises.FirstOrDefaultAsync(p => p.Nombre == "Perú");
						
						int paisId = paisPeru.Id;

						nuevasUniversidades.Add(new Universidad
						{
							Nombre = nombre,
							PaisId = paisId
						});
					}
				}
			}

			if (nuevasUniversidades.Any())
			{
				_context.Universidades.AddRange(nuevasUniversidades);
				await _context.SaveChangesAsync();
			}
		}

	}
}
