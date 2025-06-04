using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Educacion
	{
		public int Id { get; set; }
		public int PersonaId { get; set; }
		public bool EsExtranjera { get; set; }
		public int PaisUniversidadId { get; set; }
		public int UniversidadId { get; set; }
		public DateTime FechaEmisionTitulo { get; set; }
		public string TipoValidacion { get; set; } // "reconocimiento", "revalidacion"
		public string? NumeroResolucion { get; set; }
		public string? ResolucionPath { get; set; }
		public int? UniversidadPeruanaId { get; set; } // Solo para revalidación
	}
}
