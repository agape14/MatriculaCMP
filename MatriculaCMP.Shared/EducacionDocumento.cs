using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class EducacionDocumento
    {
        public int Id { get; set; }
        public int EducacionId { get; set; } // FK
        public string? TituloMedicoCirujanoPath { get; set; }
        public string? ConstanciaInscripcionSuneduPath { get; set; }
        public string? CertificadoAntecedentesPenalesPath { get; set; }
        public string? CarnetExtranjeriaPath { get; set; }
        public string? ConstanciaInscripcionReconocimientoSuneduPath { get; set; }
        public string? ConstanciaInscripcionRevalidacionUniversidadNacionalPath { get; set; }
        public string? ReconocimientoSuneduPath { get; set; }
        public string? RevalidacionUniversidadNacionalPath { get; set; }

        public Educacion? Educacion { get; set; }
    }
}
