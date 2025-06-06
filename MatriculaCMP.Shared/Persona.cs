using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class Persona
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "El consejo regional es requerido")]
		public string ConsejoRegionalId { get; set; }

		[Required(ErrorMessage = "Los nombres son requeridos")]
		[StringLength(50, ErrorMessage = "Los nombres no pueden exceder los 50 caracteres")]
		public string Nombres { get; set; }

		[Required(ErrorMessage = "El apellido paterno es requerido")]
		[StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres")]
		public string ApellidoPaterno { get; set; }

		[Required(ErrorMessage = "El apellido materno es requerido")]
		[StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres")]
		public string? ApellidoMaterno { get; set; }

		
		[Required(ErrorMessage = "El sexo es requerido")]
		public bool Sexo { get; set; } // true: Masculino, false: Femenino

		[Required(ErrorMessage = "El estado civil es requerido")]
		public string EstadoCivilId { get; set; }


		[Required(ErrorMessage = "El tipo de documento es requerido")]
		public string TipoDocumentoId { get; set; } = "62";

		[Required(ErrorMessage = "El número de documento es requerido")]
		[StringLength(20, ErrorMessage = "El número de documento no puede exceder los 20 caracteres")]
		[RegularExpression(@"^\d+$", ErrorMessage = "El número de documento debe contener solo dígitos")]
		public string NumeroDocumento { get; set; }

		[Required(ErrorMessage = "El grupo sanguíneo es requerido")]
		public string GrupoSanguineoId { get; set; } = "177";


		[Required(ErrorMessage = "La fecha de nacimiento es requerida")]
		public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-18); // Valor por defecto 18 años atrás

		
		[Required(ErrorMessage = "El país de nacimiento es requerido")]
		public string PaisNacimientoId { get; set; }

		[Required(ErrorMessage = "El departamento de nacimiento es requerido")]
		public string DepartamentoNacimientoId { get; set; } ="14";


		[Required(ErrorMessage = "La provincia de nacimiento es requerida")]
		public string ProvinciaNacimientoId { get; set; }

		[Required(ErrorMessage = "El distrito de nacimiento es requerido")]
		public string DistritoNacimientoId { get; set; }

		[StringLength(10, ErrorMessage = "El teléfono no puede exceder los 10 caracteres")]
		[RegularExpression(@"^\d*$", ErrorMessage = "El teléfono debe contener solo dígitos")]
		public string Telefono { get; set; }

		[Required(ErrorMessage = "El celular es requerido")]
		[StringLength(10, ErrorMessage = "El celular no puede exceder los 10 caracteres")]
		[RegularExpression(@"^\d+$", ErrorMessage = "El celular debe contener solo dígitos")]
		public string Celular { get; set; }

		[Required(ErrorMessage = "El correo electrónico es requerido")]
		[EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
		[StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres")]
		public string Email { get; set; }

		// Datos de Domicilio
		[Required(ErrorMessage = "La zona de domicilio es requerida")]
		public string ZonaDomicilioId { get; set; } = "8";

		[Required(ErrorMessage = "La descripción de zona es requerida")]
		[StringLength(100, ErrorMessage = "La descripción de zona no puede exceder los 100 caracteres")]
		public string DescripcionZona { get; set; }


		[Required(ErrorMessage = "La vía es requerida")]
		public string ViaDomicilioId { get; set; } = "1";

		[Required(ErrorMessage = "La descripción de vía es requerida")]
		[StringLength(100, ErrorMessage = "La descripción de vía no puede exceder los 100 caracteres")]
		public string DescripcionVia { get; set; }


		[Required(ErrorMessage = "El departamento de domicilio es requerido")]
		public string DepartamentoDomicilioId { get; set; }

		[Required(ErrorMessage = "La provincia de domicilio es requerida")]
		public string ProvinciaDomicilioId { get; set; }

		[Required(ErrorMessage = "El distrito de domicilio es requerido")]
		public string DistritoDomicilioId { get; set; }
		public string? FotoPath { get; set; }

		[JsonIgnore]
		[NotMapped] // <- Esto evita que Entity Framework intente mapearlo
		[Required(ErrorMessage = "La foto del médico es requerida y que este validada.")]
		public IBrowserFile FotoMedico { get; set; }

		// Términos y condiciones		
		[NotMapped] // <- Esto evita que Entity Framework intente mapearlo
		[Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar las políticas de privacidad")]
		public bool AceptaPoliticas { get; set; }
		public DateTime FechaRegistro { get; set; } = DateTime.Now;
	}
}
