using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class Correlativos
    {
        public int Id { get; set; }
        public string? Nombre { get; set; } = "SOLICITUD";
        public int UltimoNumero { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
