using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Ubigeo
	{
		public int Id { get; set; }
		public string Codigo { get; set; }
		public string Nombre { get; set; }
		public string Tipo { get; set; } // "departamento", "provincia", "distrito"
		public string CodigoPadre { get; set; } // Para provincias y distritos
	}
}
