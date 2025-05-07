using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatriculaCMP.Shared
{
    public class ListadoPrematricula
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public string NombreCompleto { get; set; }
        public string FechaNacimiento { get; set; }
        public string LugarNacimiento { get; set; }
        public string Estado { get; set; } // "Pagado", "sin pagar", "Desistido", etc.
        public bool Seleccionado { get; set; } // <-- NUEVO
    }
}
