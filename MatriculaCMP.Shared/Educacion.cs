using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Educacion
	{
		public int Id { get; set; }
		public int PersonaId { get; set; }

        // Propiedad de navegación
        public Persona Persona { get; set; }

        [Required(ErrorMessage = "El origen de universidad es requerido")]
		public string UniversidadOrigen { get; set; } = "1"; // Valor por defecto Nacional

		public bool EsExtranjera { get; set; }
		public int PaisUniversidadId { get; set; }

		[Required(ErrorMessage = "La universidad es requerida")]
		public int UniversidadId { get; set; }
        // 🆕 Propiedad de navegación hacia Universidad
        public Universidad Universidad { get; set; }

        [Required(ErrorMessage = "La fecha de emisión del título es requerida")]
		public DateTime FechaEmisionTitulo { get; set; }

        //[RequiredIf(nameof(TipoValidacion), "reconocimiento", ErrorMessage = "Resolución es requerida")]

        public string? TipoValidacion { get; set; } // "reconocimiento", "revalidacion"
        public int? UniversidadPeruanaId { get; set; } // Solo para revalidación
        public string? NumeroResolucion { get; set; }
        public string? ResolucionPath { get; set; }

        public EducacionDocumento? Documento { get; set; }

    }
}
