using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Mat_ConsejoRegional
	{
		public int ConsejoRegional_Key { get; set; }
		public string? ConsejoRegional_Id { get; set; }
		public string? Nombre { get; set; }
		public string? NombreAbreviado { get; set; }
		public bool? EsConsejoNacional { get; set; }
		public bool? Activo { get; set; }
		public int? Ubigeo_Key { get; set; }
	}
}
