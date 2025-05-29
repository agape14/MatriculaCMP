using MatriculaCMP.Shared;
using System.Net.Http.Json;

namespace MatriculaCMP.Client.Services
{
    public class PaisUniversidadesService
    {
        private readonly HttpClient _http;

        public PaisUniversidadesService(HttpClient http) => _http = http;

        public async Task<List<Pais>> ObtenerPaisesAsync() =>
            await _http.GetFromJsonAsync<List<Pais>>("api/pais");

        public async Task<List<Universidad>> ObtenerUniversidadesAsync(int paisId) =>
            await _http.GetFromJsonAsync<List<Universidad>>($"api/universidad/{paisId}");
    }
}
