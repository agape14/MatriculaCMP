using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Mat_Ubigeo
	{
		[Key]
		[Column("Ubigeo_Key")]
		public int UbigeoKey { get; set; }

		[Column("Ubigeo_Id")]
		[StringLength(6)]
		public string? UbigeoId { get; set; }

		[Column("UbigeoInei_Id")]
		[StringLength(6)]
		public string? UbigeoIneiId { get; set; }

		[Column("Departamento_Id")]
		[StringLength(2)]
		public string? DepartamentoId { get; set; }

		[Column("Provincia_Id")]
		[StringLength(2)]
		public string? ProvinciaId { get; set; }

		[Column("Distrito_Id")]
		[StringLength(2)]
		public string? DistritoId { get; set; }

		[Column("Nombre")]
		[StringLength(100)]
		public string? Nombre { get; set; }

		[Column("NombreUnido")]
		[StringLength(300)]
		public string? NombreUnido { get; set; }

		[Column("CodigoPostal")]
		[StringLength(30)]
		public string? CodigoPostal { get; set; }

		[Column("EsExtranjero")]
		public bool? EsExtranjero { get; set; }

		[Column("UbigeoMundial_Id")]
		[StringLength(6)]
		public string? UbigeoMundialId { get; set; }

		[Column("flg_activo")]
		public bool? FlgActivo { get; set; }
	}
}
