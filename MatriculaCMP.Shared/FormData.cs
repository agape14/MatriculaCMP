using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
	public class FormData
	{
        public int PersonaId { get; set; }
        // Datos del Consejo Regional
        [Required(ErrorMessage = "El consejo regional es requerido")]
		public string ConsejoRegional { get; set; }

		// Datos Personales
		[Required(ErrorMessage = "Los nombres son requeridos")]
		[StringLength(50, ErrorMessage = "Los nombres no pueden exceder los 50 caracteres")]
		public string Nombres { get; set; }

		[Required(ErrorMessage = "El apellido paterno es requerido")]
		[StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres")]
		public string ApellidoPaterno { get; set; }

		[Required(ErrorMessage = "El apellido materno es requerido")]
		[StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres")]
		public string ApellidoMaterno { get; set; }

		[Required(ErrorMessage = "El sexo es requerido")]
		public string Sexo { get; set; } = "1"; // Valor por defecto Masculino

		[Required(ErrorMessage = "El estado civil es requerido")]
		public string EstadoCivil { get; set; } = "69"; // Valor por defecto Soltero

		// Datos de Documento
		[Required(ErrorMessage = "El tipo de documento es requerido")]
		public string TipoDocumento { get; set; } = "62"; // Valor por defecto DNI

		[Required(ErrorMessage = "El número de documento es requerido")]
		[StringLength(20, ErrorMessage = "El número de documento no puede exceder los 20 caracteres")]
		[RegularExpression(@"^\d+$", ErrorMessage = "El número de documento debe contener solo dígitos")]
		public string NumeroDocumento { get; set; }

		[Required(ErrorMessage = "El grupo sanguíneo es requerido")]
		public string GrupoSanguineo { get; set; } = "177"; // Valor por defecto O+

		// Datos de Nacimiento
		[Required(ErrorMessage = "El país de nacimiento es requerido")]
		public string PaisNacimiento { get; set; }

		[Required(ErrorMessage = "El departamento de nacimiento es requerido")]
		public string DepartamentoNacimiento { get; set; }

		[Required(ErrorMessage = "La provincia de nacimiento es requerida")]
		public string ProvinciaNacimiento { get; set; }

		[Required(ErrorMessage = "El distrito de nacimiento es requerido")]
		public string DistritoNacimiento { get; set; }

		[Required(ErrorMessage = "La fecha de nacimiento es requerida")]
		public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-18); // Valor por defecto 18 años atrás

		// Datos de Domicilio
		[Required(ErrorMessage = "La zona de domicilio es requerida")]
		public string ZonaDomicilio { get; set; } = "8"; // Valor por defecto ASENTAMIENTO HUMANO

		[Required(ErrorMessage = "La descripción de zona es requerida")]
		[StringLength(100, ErrorMessage = "La descripción de zona no puede exceder los 100 caracteres")]
		public string DescripcionZona { get; set; }

		[Required(ErrorMessage = "La vía es requerida")]
		public string ViaDomicilio { get; set; } = "1"; // Valor por defecto AVENIDA

		[Required(ErrorMessage = "La descripción de vía es requerida")]
		[StringLength(100, ErrorMessage = "La descripción de vía no puede exceder los 100 caracteres")]
		public string DescripcionVia { get; set; }

		[Required(ErrorMessage = "El departamento de domicilio es requerido")]
		public string DepartamentoDomicilio { get; set; } = "14"; // Valor por defecto LIMA

		[Required(ErrorMessage = "La provincia de domicilio es requerida")]
		public string ProvinciaDomicilio { get; set; }

		[Required(ErrorMessage = "El distrito de domicilio es requerido")]
		public string DistritoDomicilio { get; set; }

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

		// Datos de Universidad
		[Required(ErrorMessage = "El origen de universidad es requerido")]
		public string UniversidadOrigen { get; set; } = "1"; // Valor por defecto Nacional

		public string PaisUniversidad { get; set; }

		/// <summary>UniversidadId (Nacional). Solo obligatorio cuando UniversidadOrigen == "1". Si es Extranjera se usa NombreUniversidadExtranjera.</summary>
		public string Universidad { get; set; }

		public string? NombreUniversidadExtranjera { get; set; }

		/// <summary>Requerido en Paso 3 (fecha de titulación). No se pide en Paso 1.</summary>
		public DateTime FechaEmisionTitulo { get; set; } = default(DateTime);

		public string TipoValidacion { get; set; } // Solo para extranjeras
		public string NumeroResolucion { get; set; } // Solo para extranjeras
		public string? ResolucionPath { get; set; }
		public string UniversidadPeruana { get; set; } // Solo para revalidación
		public DateTime? FechaReconocimiento { get; set; } // Reconocimiento Sunedu
		public DateTime? FechaRevalidacion { get; set; } // Revalidación

		// Términos y condiciones
		[Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar las políticas de privacidad")]
		public bool AceptaPoliticas { get; set; }

		//[Required(ErrorMessage = "La foto del médico es requerida y que este validada")]
		public IBrowserFile FotoMedico { get; set; }
        public IBrowserFile? TituloMedicoCirujanoPath { get; set; }
        public IBrowserFile? ConstanciaInscripcionSuneduPath { get; set; }
        public IBrowserFile? CertificadoAntecedentesPenalesPath { get; set; }
        public IBrowserFile? CarnetExtranjeriaPath { get; set; }
        public IBrowserFile? ConstanciaInscripcionReconocimientoSuneduPath { get; set; }
        public IBrowserFile? ConstanciaInscripcionRevalidacionUniversidadNacionalPath { get; set; }
        public IBrowserFile? ReconocimientoSuneduPath { get; set; }
        public IBrowserFile? RevalidacionUniversidadNacionalPath { get; set; }

	}
}
