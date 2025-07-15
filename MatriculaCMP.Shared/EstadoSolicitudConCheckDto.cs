using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class EstadoSolicitudConCheckDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool TieneCheck { get; set; }
        public bool VerReporte { get; set; }
        
        public string Color { get; set; } = string.Empty;
    }
}
