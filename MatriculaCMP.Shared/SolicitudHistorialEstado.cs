using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class SolicitudHistorialEstado
    {
        public int Id { get; set; }

        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; }

        public int? EstadoAnteriorId { get; set; }
        public EstadoSolicitud? EstadoAnterior { get; set; }

        public int EstadoNuevoId { get; set; }
        public EstadoSolicitud EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.Now;

        public string? Observacion { get; set; }

        public string? UsuarioCambio { get; set; }
    }
}
