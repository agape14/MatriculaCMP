using MatriculaCMP.Shared;

namespace MatriculaCMP.Client.Services;

/// <summary>
/// Caché en memoria del menú por perfil para que el menú lateral se muestre de inmediato al navegar,
/// sin efecto de recarga mientras se vuelve a solicitar la API.
/// </summary>
public class MenuCacheService
{
    private readonly Dictionary<int, List<Menu>> _cache = new();
    private readonly object _lock = new();

    public List<Menu>? GetCached(int perfilId)
    {
        lock (_lock)
        {
            return _cache.TryGetValue(perfilId, out var list) ? list : null;
        }
    }

    public void SetCached(int perfilId, List<Menu> menus)
    {
        if (menus == null) return;
        lock (_lock)
        {
            _cache[perfilId] = menus;
        }
    }

    public void Clear(int? perfilId = null)
    {
        lock (_lock)
        {
            if (perfilId.HasValue)
                _cache.Remove(perfilId.Value);
            else
                _cache.Clear();
        }
    }
}
