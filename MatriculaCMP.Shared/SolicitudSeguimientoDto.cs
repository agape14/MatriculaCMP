using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class SolicitudSeguimientoDto
    {
        public int Id { get; set; }
        public string TipoSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; }
        public string AreaNombre { get; set; }
        public string Observaciones { get; set; }
    }
}
