using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    [Table("Usuario", Schema = "Seguridad")]
    public class UsuarioSGD
    {
        [Key]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }

        [Column("IdPersona")]
        public int? IdPersona { get; set; }

        [Column("IdCatalogoTipoUsuario")]
        public int? IdCatalogoTipoUsuario { get; set; }

        [Column("Logueo")]
        [StringLength(100)]
        public string? Logueo { get; set; }

        [Column("Clave")]
        [StringLength(4000)]
        public string? Clave { get; set; }

        [Column("ClaveAnterior")]
        [StringLength(4000)]
        public string? ClaveAnterior { get; set; }

        [Column("NumeroCambioClave")]
        public int? NumeroCambioClave { get; set; }

        [Column("Bloqueado")]
        public bool? Bloqueado { get; set; }

        [Column("Email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("RutaArchivoFoto")]
        [StringLength(50)]
        public string? RutaArchivoFoto { get; set; }

        [Column("Verificado")]
        public bool? Verificado { get; set; }

        [Column("EsInstitucion")]
        public bool? EsInstitucion { get; set; }

        [Column("IdUsuarioCreacionAuditoria")]
        public int? IdUsuarioCreacionAuditoria { get; set; }

        [Column("FechaCreacionAuditoria")]
        public DateTime? FechaCreacionAuditoria { get; set; }

        [Column("IdUsuarioActualizacionAuditoria")]
        public int? IdUsuarioActualizacionAuditoria { get; set; }

        [Column("FechaActualizacionAuditoria")]
        public DateTime? FechaActualizacionAuditoria { get; set; }

        [Column("EstadoAuditoria")]
        public bool? EstadoAuditoria { get; set; }

        [Column("FlagPrivMarcaAgua")]
        public bool FlagPrivMarcaAgua { get; set; }

        [ForeignKey("IdPersona")]
        public PersonaSGD? PersonaSGD { get; set; }
    }
}
