using System.Net.Http.Json;

namespace MatriculaCMP.Client.Services
{
    public class MenuHttpService
    {
        private readonly HttpClient _http;

        public MenuHttpService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MenuDto>> ObtenerMenusPorPerfilAsync(int perfilId)
        {
            return await _http.GetFromJsonAsync<List<MenuDto>>($"api/menu/por-perfil/{perfilId}");
        }
    }

    public class MenuDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Ruta { get; set; }
        public string Modulo { get; set; }
        public string Icono { get; set; }
    }
}
