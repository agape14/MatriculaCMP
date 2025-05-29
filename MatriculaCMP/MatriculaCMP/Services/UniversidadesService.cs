using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System;

namespace MatriculaCMP.Services
{
    public class UniversidadesService
    {
        private readonly HttpClient _http;
        private readonly ApplicationDbContext _context;

        public UniversidadesService(HttpClient http, ApplicationDbContext context)
        {
            _http = http;
            _context = context;
        }

        public async Task ObtenerYPersistirUniversidadesAsync()
        {
            var paises = await _context.Paises.ToListAsync();

            foreach (var pais in paises)
            {
                var response = await _http.GetStringAsync($"http://universities.hipolabs.com/search?country={pais.Nombre}");
                dynamic universidades = JsonConvert.DeserializeObject(response);

                foreach (var u in universidades)
                {
                    string nombreUni = u.name.ToString();
                    if (!_context.Universidades.Any(x => x.Nombre == nombreUni && x.PaisId == pais.Id))
                    {
                        _context.Universidades.Add(new Universidad
                        {
                            Nombre = nombreUni,
                            PaisId = pais.Id
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
