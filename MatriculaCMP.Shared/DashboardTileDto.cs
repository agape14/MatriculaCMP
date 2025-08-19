using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public record DashboardTileDto(string titulo, int valor, string color, string icono, string? colortexto);
}
