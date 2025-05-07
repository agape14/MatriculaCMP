using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Services
{
    public class MenuService
    {
        private readonly ApplicationDbContext _context;
        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Menu>> ObtenerMenusPorPerfilAsync(int perfilId)
        {
            return await _context.PerfilMenu
            .Where(pm => pm.PerfilId == perfilId)
            .Include(pm => pm.Menu)
            .Select(pm => new Menu
            {
                Id = pm.Menu.Id,
                Titulo = pm.Menu.Titulo ?? string.Empty,
                Icono = pm.Menu.Icono ?? string.Empty,
                Ruta = pm.Menu.Ruta ?? string.Empty,
                Modulo = pm.Menu.Modulo ?? string.Empty
            })
            .OrderBy(m => m.Modulo)
            .ToListAsync();
        }
    }
}
