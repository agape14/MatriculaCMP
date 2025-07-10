using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MatriculaCMP.Shared
{
    public class Persona
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "El consejo regional es requerido")]
        public string? ConsejoRegionalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(50)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es requerido")]
        [StringLength(50)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es requerido")]
        [StringLength(50)]
        public string? ApellidoMaterno { get; set; }

        [NotMapped]
        public string NombresCompletos => $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}";

        // Campos que podrían ser opcionales para perfiles no médicos
        public bool? Sexo { get; set; }

        public string? EstadoCivilId { get; set; }

        public string? TipoDocumentoId { get; set; } = "62";

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Solo dígitos")]
        public string NumeroDocumento { get; set; } = string.Empty;

        public int GrupoSanguineoId { get; set; } = 177;

        [ForeignKey(nameof(GrupoSanguineoId))]
        public MaestroRegistro GrupoSanguineo { get; set; }
        public DateTime? FechaNacimiento { get; set; } = new DateTime(1990, 1, 1);

        public string? PaisNacimientoId { get; set; }

        public string? DepartamentoNacimientoId { get; set; } = "14";
        public string? ProvinciaNacimientoId { get; set; }
        public string? DistritoNacimientoId { get; set; }

        [StringLength(10)]
        public string? Telefono { get; set; }

        //[Required]
        [StringLength(10)]
        [RegularExpression(@"^\d+$")]
        public string? Celular { get; set; } = string.Empty;

        //[Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; } = string.Empty;

        // Domicilio
        public string? ZonaDomicilioId { get; set; } = "8";
        [StringLength(100)]
        public string? DescripcionZona { get; set; }

        public string? ViaDomicilioId { get; set; } = "1";
        [StringLength(100)]
        public string? DescripcionVia { get; set; }

        public string? DepartamentoDomicilioId { get; set; }
        public string? ProvinciaDomicilioId { get; set; }
        public string? DistritoDomicilioId { get; set; }

        public string? FotoPath { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Upload (no mapeado)
        [JsonIgnore, NotMapped]
        public IBrowserFile? FotoMedico { get; set; }

        [NotMapped]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar las políticas de privacidad")]
        public bool AceptaPoliticas { get; set; }

        // Relaciones
        public List<Educacion> Educaciones { get; set; } = new();
        public List<Usuario> Usuarios { get; set; } = new();         // Nueva relación
        public List<Solicitud> Solicitudes { get; set; } = new();    // Nueva relación
    }
}
