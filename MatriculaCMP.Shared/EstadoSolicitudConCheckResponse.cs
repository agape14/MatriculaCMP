using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class EstadoSolicitudConCheckResponse
    {
        public double PorcentajeCompletado { get; set; }
        public List<EstadoSolicitudConCheckDto> Estados { get; set; } = new();
    }
}
