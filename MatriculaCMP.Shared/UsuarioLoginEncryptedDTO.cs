using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class UsuarioLoginEncryptedDTO
    {
        public string TipoDocumentoEncrypted { get; set; }
        public string NumeroDocumentoEncrypted { get; set; }
        /// <summary>
        /// Id del perfil con el que se desea iniciar sesión (ej. 2 = Médico, 1 = Admin).
        /// Si no se envía, por defecto se usa 2 (Médico). Requerido para integración desde INTRANET u otros proyectos.
        /// </summary>
        public int? PerfilId { get; set; }
    }
}
