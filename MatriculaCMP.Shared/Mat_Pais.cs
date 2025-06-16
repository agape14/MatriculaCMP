using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Mat_Pais
	{
		[Key]
		[Column("Pais_key")]
		public int Pais_key { get; set; }

		[Column("Iso3")]
		[StringLength(3)]
		public string Iso3 { get; set; }

		[Column("Nombre")]
		[StringLength(100)]
		public string Nombre { get; set; }

		[Column("bActivo")]
		public bool? Activo { get; set; }
	}
}
