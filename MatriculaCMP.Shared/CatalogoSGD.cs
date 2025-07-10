using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    [Table("Catalogo", Schema = "General")]
    public class CatalogoSGD
    {
        [Key]
        public int IdCatalogo { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdCatalogoPadre { get; set; }

        public int? OrdenItem { get; set; }

        [StringLength(100)]
        public string? Descripcion { get; set; }

        [StringLength(250)]
        public string? Detalle { get; set; }

        [StringLength(250)]
        public string? Detalle1 { get; set; }

        public bool? Activo { get; set; }

        public int? IdUsuarioCreacionAuditoria { get; set; }

        public DateTime? FechaCreacionAuditoria { get; set; }

        public int? IdUsuarioActualizacionAuditoria { get; set; }

        public DateTime? FechaActualizacionAuditoria { get; set; }

        public bool? EstadoAuditoria { get; set; }
    }
}
