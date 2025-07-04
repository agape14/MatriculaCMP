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
    public class UsuarioInsert
    {
        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(50)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es requerido")]
        [StringLength(50)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es requerido")]
        [StringLength(50)]
        public string? ApellidoMaterno { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Solo dígitos")]
        public string NumeroDocumento { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;
        [NotMapped]
        public string Password { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;

        public int PerfilId { get; set; }
        [JsonIgnore]
        public Perfil? Perfil { get; set; }

        public int? PersonaId { get; set; }
        [JsonIgnore]
        public Persona? Persona { get; set; }
    }
}
