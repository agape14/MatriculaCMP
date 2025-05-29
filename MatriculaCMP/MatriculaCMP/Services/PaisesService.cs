using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace MatriculaCMP.Services
{
    public class PaisesService
    {
        private readonly HttpClient _http;
        private readonly ApplicationDbContext _context;

        public PaisesService(HttpClient http, ApplicationDbContext context)
        {
            _http = http;
            _context = context;
        }

		public async Task ObtenerYPersistirPaisesAsync()
		{
			var response = await _http.GetStringAsync("https://restcountries.com/v3.1/all");
			dynamic datos = JsonConvert.DeserializeObject(response);

			// Traer todos los códigos existentes UNA SOLA VEZ
			var codigosExistentes = await _context.Paises
				.Select(p => p.Codigo)
				.ToListAsync();

			var nuevosPaises = new List<Pais>();

			foreach (var pais in datos)
			{
				string nombre = pais.translations?.spa?.common != null
					? pais.translations.spa.common.ToString()
					: pais.name.common.ToString();

				string codigo = pais.cca2;

				if (!codigosExistentes.Contains(codigo))
				{
					nuevosPaises.Add(new Pais { Nombre = nombre, Codigo = codigo });
				}
			}

			if (nuevosPaises.Any())
			{
				int batchSize = 50;
				for (int i = 0; i < nuevosPaises.Count; i += batchSize)
				{
					var batch = nuevosPaises.Skip(i).Take(batchSize).ToList();
					_context.Paises.AddRange(batch);
					await _context.SaveChangesAsync();
				}
			}
		}



	}
}
