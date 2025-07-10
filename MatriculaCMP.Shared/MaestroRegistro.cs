using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class MaestroRegistro
	{
        [Key]
        public int MaestroRegistro_Key { get; set; }
		public int MaestroTabla_Key { get; set; }
		public string? Codigo { get; set; }
		public string? Nombre { get; set; }
	}
}
