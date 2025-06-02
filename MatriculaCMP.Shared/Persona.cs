using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Persona
	{
		public int Id { get; set; }
		public int ConsejoRegionalId { get; set; }
		public string Nombres { get; set; }
		public string ApellidoPaterno { get; set; }
		public string ApellidoMaterno { get; set; }
		public bool Sexo { get; set; } // true: Masculino, false: Femenino
		public int EstadoCivilId { get; set; }
		public int TipoDocumentoId { get; set; }
		public string NumeroDocumento { get; set; }
		public int GrupoSanguineoId { get; set; }
		public DateTime FechaNacimiento { get; set; }
		public int PaisNacimientoId { get; set; }
		public int DepartamentoNacimientoId { get; set; }
		public int ProvinciaNacimientoId { get; set; }
		public int DistritoNacimientoId { get; set; }
		public string Telefono { get; set; }
		public string Celular { get; set; }
		public string Email { get; set; }
		public int ZonaDomicilioId { get; set; }
		public string DescripcionZona { get; set; }
		public int ViaDomicilioId { get; set; }
		public string DescripcionVia { get; set; }
		public int DepartamentoDomicilioId { get; set; }
		public int ProvinciaDomicilioId { get; set; }
		public int DistritoDomicilioId { get; set; }
		public string? FotoPath { get; set; }
		public DateTime FechaRegistro { get; set; } = DateTime.Now;
	}
}
