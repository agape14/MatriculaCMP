using System;

namespace MatriculaCMP.Shared
{
    public class EstadoMesSerieDto
    {
        public int EstadoId { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public int[] Meses { get; set; } = new int[12];
    }
}


