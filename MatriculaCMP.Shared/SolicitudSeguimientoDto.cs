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
        public int NumeroSolicitud { get; set; }
        public string TipoSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int EstadoId { get; set; }
		public string Estado { get; set; }
		public string EstadoColor { get; set; }
		public string AreaNombre { get; set; }
        public string PersonaNombre { get; set; }
        public string UniversidadNombre { get; set; }
        public string Observaciones { get; set; }
    }
}
