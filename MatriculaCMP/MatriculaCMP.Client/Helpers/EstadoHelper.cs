namespace MatriculaCMP.Client.Helpers
{
    public static  class EstadoHelper
    {
         public static string GetBadgeColor(int estado)
        {
            return estado switch
            {
                1 => "primary", // Registrado
                2 => "success",   // Aprobado por Consejo Regional
                3 => "danger",    // Rechazado por Consejo Regional
                4 => "success",   // Aprobado por Secretaria General
                5 => "danger",    // Rechazado por Secretaria General
                6 => "success",   // Aprobado por Of. Matrícula
                7 => "danger",    // Rechazado por Of. Matrícula
                8 => "warning",   // Pendiente Firma Secretario CR
                9 => "warning",   // Pendiente Firma Decano CR
                10 => "warning",  // Pendiente Firma Secretario General
                11 => "warning",  // Pendiente Firma Decano
                12 => "info",     // Diploma Firmado - Pendiente Entrega
                13 => "success",  // Proceso finalizado - Entregado
                _ => "light"
            };
        }
    }
}
