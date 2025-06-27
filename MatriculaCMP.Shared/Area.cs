using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class Area
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty; // Ej. "Consejo Regional", "Decanato", etc.

        public List<Solicitud> Solicitudes { get; set; } = new();
    }
}
