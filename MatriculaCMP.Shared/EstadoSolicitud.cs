using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class EstadoSolicitud
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty; // Ej. "Registrado", "En revisión por CR", etc.
        public string Color { get; set; } = string.Empty;

        public List<Solicitud> Solicitudes { get; set; } = new();
    }
}
