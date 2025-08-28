using MatriculaCMP.Services;
using MatriculaCMP.Shared;
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

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var menus = await _menuService.ObtenerTodosMenusAsync();
            return Ok(menus);
        }

        [HttpGet("ids-por-perfil/{perfilId}")]
        public async Task<IActionResult> ObtenerIdsPorPerfil(int perfilId)
        {
            var ids = await _menuService.ObtenerMenuIdsPorPerfilAsync(perfilId);
            return Ok(ids);
        }

        public class AsignacionRequest { public int PerfilId { get; set; } public List<int> MenuIds { get; set; } = new(); }

        [HttpPost("asignar")]
        public async Task<IActionResult> AsignarMenus([FromBody] AsignacionRequest req)
        {
            if (req == null || req.PerfilId <= 0) return BadRequest("Solicitud inválida");
            await _menuService.ActualizarAsignacionesAsync(req.PerfilId, req.MenuIds ?? new List<int>());
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Menu menu)
        {
            if (menu == null) return BadRequest("Datos inválidos");
            var creado = await _menuService.CrearMenuAsync(menu);
            return Ok(creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Menu menu)
        {
            if (menu == null) return BadRequest("Datos inválidos");
            var ok = await _menuService.ActualizarMenuAsync(id, menu);
            return ok ? Ok(new { success = true }) : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var ok = await _menuService.EliminarMenuAsync(id);
            return ok ? Ok(new { success = true }) : NotFound();
        }

        public class ModuloIconoRequest { public string Modulo { get; set; } = string.Empty; public string ModuloIcono { get; set; } = string.Empty; }

        [HttpPost("modulo-icono")] // api/menu/modulo-icono
        public async Task<IActionResult> ActualizarModuloIcono([FromBody] ModuloIconoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Modulo)) return BadRequest("Datos inválidos");
            await _menuService.ActualizarModuloIconoAsync(req.Modulo, req.ModuloIcono ?? string.Empty);
            return Ok(new { success = true });
        }
    }
}
