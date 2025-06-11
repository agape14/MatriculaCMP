using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class PersonaSGD
    {
        [Key]
        public int IdPersona { get; set; }
        public int IdCatalogoTipoPersona { get; set; }
        public string NombreCompleto { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int IdCatalogoTipoDocumentoPersonal { get; set; }
        public string NumeroDocumento { get; set; }
        public string Direccion { get; set; }
        public string Celular { get; set; }
        public string? FechaNacimiento { get; set; } // ✅

        public bool Sexo { get; set; }
    }
}
