using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class FirmaDigitalDTO
    {
        public class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
        }

        public class DownloadResponse
        {
            public int estado { get; set; }
            public string descripcion { get; set; }
            public string archivoFirmado { get; set; }
        }

        public class UploadResponse
        {
            public int codigoFirma { get; set; }
            public string descripcion { get; set; }
        }

        public class FirmaRequest
        {
            public int IdExpedienteDocumento { get; set; }
            public int IdExpedienteDocumentoFirmante { get; set; }
            public int TipoDocumentoFirmado { get; set; }
            public int IdUsuario { get; set; }
            public int IdExpedienteDocumentoAdjunto { get; set; }
            public int IdAvalMedico { get; set; }
        }

        public class UploadRequest
        {
            public int IdExpedienteDocumento { get; set; }
            public int IdExpedienteDocumentoFirmante { get; set; }
            public int CodigoFirma { get; set; }
        }

        public class Mensaje
        {
            public int CodigoMensaje { get; set; }
            public string DescripcionMensaje { get; set; }
        }
    }
}
