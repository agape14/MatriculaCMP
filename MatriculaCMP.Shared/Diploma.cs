using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatriculaCMP.Shared
{
    public class Diploma
    {
        public int Id { get; set; }
        
        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; } = default!;
        
        public int PersonaId { get; set; }
        public Persona Persona { get; set; } = default!;
        
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [Required]
        public string NumeroColegiatura { get; set; } = string.Empty;
        
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        
        public string? RutaPdf { get; set; }
        public string? RutaPdfFirmado { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public string? UsuarioCreacion { get; set; }

        [NotMapped]
        public string? UniversidadNombre { get; set; }
    }
}
