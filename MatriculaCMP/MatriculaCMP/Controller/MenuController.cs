using MatriculaCMP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatriculaCMP.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly MenuService _menuService;

        public MenuController(MenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("por-perfil/{perfilId}")]
        public async Task<IActionResult> ObtenerMenusPorPerfil(int perfilId)
        {
            var menus = await _menuService.ObtenerMenusPorPerfilAsync(perfilId);
            return Ok(menus);
        }
    }
}
