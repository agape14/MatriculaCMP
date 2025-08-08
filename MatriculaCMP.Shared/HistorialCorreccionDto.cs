using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class HistorialCorreccionDto
    {
        public DateTime Fecha { get; set; }
        public string EstadoAnterior { get; set; } = "";
        public string EstadoNuevo { get; set; } = "";
        public string Observacion { get; set; } = "";
        public string Usuario { get; set; } = "";
    }
}
