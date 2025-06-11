using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class PersonaConEducacionDto
    {
        public int PersonaId { get; set; }
        public string NombresCompletos { get; set; }
        public string Documento { get; set; }

        public string Celular { get; set; }
        public string Email { get; set; }
        public string Universidad { get; set; }
        public string Estado { get; set; }

        public bool Seleccionado { get; set; }
        public DateTime FechaTitulo { get; set; }
        public List<EducacionDto> Educaciones { get; set; }
    }
}
