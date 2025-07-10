using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class SolicitudCambioEstadoDto
    {
        public int SolicitudId { get; set; }
        public int NuevoEstadoId { get; set; }
        public string? Observacion { get; set; }
        public string UsuarioCambio { get; set; } = string.Empty;
    }
}
