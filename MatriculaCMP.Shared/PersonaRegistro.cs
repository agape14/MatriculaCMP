using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class PersonaRegistro
    {
        [Required(ErrorMessage = "Nombres completos son obligatorios")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Apellido paterno es obligatorio")]
        public string ApellidoPaterno { get; set; }

        [Required(ErrorMessage = "Apellido materno es obligatorio")]
        public string ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "Tipo de documento es obligatorio")]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "Número de documento es obligatorio")]
        public string NumeroDocumento { get; set; }

        [Required(ErrorMessage = "Correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Teléfono es obligatorio")]
        [Phone(ErrorMessage = "Teléfono no válido")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Código de verificación es obligatorio")]
        public string CodigoVerificacion { get; set; }

        public string Usuario { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
