using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class Solicitud
    {
        public int Id { get; set; }

        public int PersonaId { get; set; }
        public Persona Persona { get; set; } = default!;

        public string TipoSolicitud { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public int EstadoSolicitudId { get; set; }
        public EstadoSolicitud EstadoSolicitud { get; set; } = default!;

        public int? AreaId { get; set; }
        public Area? Area { get; set; }

        public string? Observaciones { get; set; }
    }
}
