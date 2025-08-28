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
                Modulo = pm.Menu.Modulo ?? string.Empty,
                ModuloIcono = pm.Menu.ModuloIcono,
                Orden = pm.Menu.Orden
            })
            .OrderBy(m => m.Modulo)
            .ThenBy(m => m.Orden ?? int.MaxValue)
            .ThenBy(m => m.Id)
            .ToListAsync();
        }

        public async Task<List<Menu>> ObtenerTodosMenusAsync()
        {
            return await _context.Menu
                .AsNoTracking()
                .OrderBy(m => m.Modulo)
                .ThenBy(m => m.Orden ?? int.MaxValue)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }

        public async Task<List<int>> ObtenerMenuIdsPorPerfilAsync(int perfilId)
        {
            return await _context.PerfilMenu
                .Where(pm => pm.PerfilId == perfilId)
                .Select(pm => pm.MenuId)
                .ToListAsync();
        }

        public async Task ActualizarAsignacionesAsync(int perfilId, List<int> menuIds)
        {
            var existentes = await _context.PerfilMenu
                .Where(pm => pm.PerfilId == perfilId)
                .ToListAsync();

            var actuales = existentes.Select(e => e.MenuId).ToHashSet();
            var nuevos = menuIds.ToHashSet();

            var aEliminar = existentes.Where(e => !nuevos.Contains(e.MenuId)).ToList();
            var aAgregar = nuevos.Where(id => !actuales.Contains(id))
                                 .Select(id => new PerfilMenu { PerfilId = perfilId, MenuId = id })
                                 .ToList();

            if (aEliminar.Any()) _context.PerfilMenu.RemoveRange(aEliminar);
            if (aAgregar.Any()) await _context.PerfilMenu.AddRangeAsync(aAgregar);

            await _context.SaveChangesAsync();
        }

        public async Task<Menu> CrearMenuAsync(Menu menu)
        {
            await _context.Menu.AddAsync(menu);
            await _context.SaveChangesAsync();
            return menu;
        }

        public async Task<bool> ActualizarMenuAsync(int id, Menu menu)
        {
            var existente = await _context.Menu.FirstOrDefaultAsync(m => m.Id == id);
            if (existente == null) return false;
            existente.Titulo = menu.Titulo;
            existente.Ruta = menu.Ruta;
            existente.Modulo = menu.Modulo;
            existente.Icono = menu.Icono;
            existente.Orden = menu.Orden;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarMenuAsync(int id)
        {
            var existente = await _context.Menu.FirstOrDefaultAsync(m => m.Id == id);
            if (existente == null) return false;

            // también eliminar asignaciones relacionadas
            var asignaciones = await _context.PerfilMenu.Where(pm => pm.MenuId == id).ToListAsync();
            if (asignaciones.Any()) _context.PerfilMenu.RemoveRange(asignaciones);

            _context.Menu.Remove(existente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ActualizarModuloIconoAsync(string modulo, string moduloIcono)
        {
            if (string.IsNullOrWhiteSpace(modulo)) return 0;
            var items = await _context.Menu
                .Where(m => m.Modulo == modulo)
                .ToListAsync();
            foreach (var m in items)
            {
                m.ModuloIcono = moduloIcono;
            }
            return await _context.SaveChangesAsync();
        }
    }
}
