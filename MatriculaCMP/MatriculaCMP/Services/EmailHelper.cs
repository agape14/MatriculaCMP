using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace MatriculaCMP.Services
{
    public class EmailHelper
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static async Task EnviarCorreoCambioEstadoAsync(string destinatario, string nombre, string apellido, int estadoId, string? observacion = null)
        {
            // Solo enviar correos para estados específicos: 1, 6, 7, 9, 11, 13
            var estadosPermitidos = new[] { 1, 6, 7, 9, 11, 13 };
            if (!estadosPermitidos.Contains(estadoId))
            {
                return; // No enviar correo para estados no permitidos
            }

            string nombreEstado = ObtenerNombreEstado(estadoId);
            string asunto = $"Estado de solicitud - {nombreEstado.ToUpper()}";
            string contenidoHtml = GetPlantillaHtml(nombre, apellido, estadoId, nombreEstado, observacion);

            using var message = new MailMessage();
            message.To.Add(new MailAddress(destinatario));
            message.From = new MailAddress("ti05@cmp.org.pe", "CMP - Colegio Médico del Perú");
            message.Subject = asunto;
            message.Body = contenidoHtml;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient("smtp.office365.com", 587)
            {
                Credentials = new NetworkCredential("ti05@cmp.org.pe", "Agape1426**"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }

        private static string ObtenerNombreEstado(int estadoId)
        {
            return estadoId switch
            {
                1 => "Registrado",
                6 => "Aprobado por Of. Matrícula",
                7 => "Rechazado por Of. Matrícula",
                9 => "Pendiente Firma Decano CR",
                11 => "Pendiente Firma Decano",
                13 => "Proceso finalizado - Entregado",
                _ => "Sin Estado"
            };
        }

        private static string GetPlantillaHtml(string nombre, string apellido, int estadoId, string nombreEstado, string? observacion = null)
        {
            string plataformaUrl = _configuration?["AppSettings:PlataformaUrl"] ?? "https://sistema.cmp.org.pe";
            // Usar URL absoluta del logo - URL pública del CMP que es accesible desde correos
            string logoUrl = "https://www.cmp.org.pe/wp-content/uploads/2020/01/logotipo-cmp.png";
            
            // Determinar el tipo de estado según el ID
            bool esAprobacion = estadoId == 6;
            bool esRechazo = estadoId == 7;
            bool esRegistrado = estadoId == 1;
            bool esFirmaDecanoCR = estadoId == 9;
            bool esFirmaDecano = estadoId == 11;
            bool esEntregado = estadoId == 13;
            bool tieneObservacion = !string.IsNullOrEmpty(observacion);
            
            // Contenido del estado según el ID
            string contenidoEstado = estadoId switch
            {
                1 => $@"<p>Su solicitud está en estado <strong style='color: #ff9800;'>{nombreEstado}</strong>.</p>
                     <p>Su solicitud ha sido registrada y está en proceso de revisión por la oficina de matrícula.</p>",
                6 => $@"<p>Su solicitud ha sido <strong style='color: #2e7d32;'>{nombreEstado}</strong>.</p>
                     <p>Su solicitud ha sido procesada exitosamente y continuará con el siguiente paso del proceso de firmas.</p>",
                7 => $@"<p>Su solicitud ha sido <strong style='color: #d32f2f;'>{nombreEstado}</strong>.</p>
                     <p>Es necesario que pueda subsanar las observaciones para continuar con el proceso.</p>",
                9 => $@"<p>Su solicitud está en estado <strong style='color: #1976d2;'>{nombreEstado}</strong>.</p>
                     <p>Su diploma está siendo firmado por el Decano del Consejo Regional. Continuará con el siguiente paso del proceso.</p>",
                11 => $@"<p>Su solicitud está en estado <strong style='color: #1976d2;'>{nombreEstado}</strong>.</p>
                     <p>Su diploma está siendo firmado por el Decano. Una vez completada la firma, su diploma estará listo para entrega.</p>",
                13 => $@"<p>Su solicitud ha alcanzado el estado <strong style='color: #2e7d32;'>{nombreEstado}</strong>.</p>
                     <p>¡Felicidades! Su proceso de colegiatura ha sido completado exitosamente. Su diploma ha sido entregado.</p>",
                _ => $@"<p>Su solicitud está en estado <strong>{nombreEstado}</strong>.</p>"
            };

            // Solo mostrar observaciones para rechazos
            string contenidoObservacion = (tieneObservacion && esRechazo) 
                ? $@"<div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                        <h4 style='color: #856404; margin: 0 0 10px 0;'>📋 Observaciones:</h4>
                        <p style='color: #856404; margin: 0;'>{observacion}</p>
                     </div>"
                : "";

            // Solo mostrar botón de acción para rechazos
            string botonAccion = esRechazo 
                ? $@"<a href='{plataformaUrl}/solicitudes/prematricula' style='display: inline-block; background-color: #681E5B; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>
                        🔧 Corregir Solicitud
                     </a>"
                : "";

            // Información adicional solo para rechazos
            string informacionAdicional = esRechazo 
                ? "<p style='color: #856404; font-size: 14px;'><strong>⚠️ Importante:</strong> Una vez corregidas las observaciones, su solicitud será reenviada para nueva revisión.</p>"
                : "";

            return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background-color: #f8f9fa;
                    margin: 0;
                    padding: 0;
                    line-height: 1.6;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    border-radius: 12px;
                    overflow: hidden;
                    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background: linear-gradient(135deg, #681E5B 0%, #8B2E7B 100%);
                    color: white;
                    padding: 30px 20px;
                    text-align: center;
                }}
                .header h1 {{
                    margin: 0;
                    font-size: 24px;
                    font-weight: 600;
                }}
                .content {{
                    padding: 30px 20px;
                }}
                .status-badge {{
                    display: inline-block;
                    padding: 8px 16px;
                    border-radius: 20px;
                    font-weight: bold;
                    font-size: 14px;
                    margin: 10px 0;
                }}
                .status-approved {{
                    background-color: #d4edda;
                    color: #155724;
                    border: 1px solid #c3e6cb;
                }}
                .status-observed {{
                    background-color: #fff3cd;
                    color: #856404;
                    border: 1px solid #ffeaa7;
                }}
                .btn-primary {{
                    background-color: #681E5B;
                    color: white;
                    padding: 12px 24px;
                    text-decoration: none;
                    border-radius: 6px;
                    font-weight: bold;
                    display: inline-block;
                    margin: 20px 0;
                    transition: background-color 0.3s ease;
                }}
                .btn-primary:hover {{
                    background-color: #5a1a4d;
                }}
                .footer {{
                    background-color: #f8f9fa;
                    padding: 20px;
                    text-align: center;
                    border-top: 1px solid #e9ecef;
                }}
                .footer p {{
                    margin: 5px 0;
                    color: #6c757d;
                    font-size: 14px;
                }}
                .logo {{
                    width: 120px;
                    height: auto;
                    margin-bottom: 15px;
                }}
                .divider {{
                    height: 1px;
                    background-color: #e9ecef;
                    margin: 20px 0;
                }}
                .info-box {{
                    background-color: #e3f2fd;
                    border: 1px solid #bbdefb;
                    border-radius: 8px;
                    padding: 15px;
                    margin: 20px 0;
                }}
                .info-box h4 {{
                    margin: 0 0 10px 0;
                    color: #1976d2;
                }}
                .info-box p {{
                    margin: 0;
                    color: #1976d2;
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='header'>
                    <img src='{logoUrl}' alt='Logo CMP' class='logo' style='max-width: 120px; height: auto; display: block; margin: 0 auto 15px;'>
                    <h1>Colegio Médico del Perú</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Sistema de Matrícula</p>
                </div>
                
                <div class='content'>
                    <h2 style='color: #681E5B; margin-bottom: 20px;'>Hola {nombre} {apellido}</h2>
                    
                    {contenidoEstado}
                    
                    <div class='status-badge {(esAprobacion || esRegistrado || esFirmaDecanoCR || esFirmaDecano || esEntregado ? "status-approved" : "status-observed")}'>
                        Estado: {nombreEstado.ToUpper()}
                    </div>
                    
                    {contenidoObservacion}
                    
                    {(!esRechazo ? @"<div class='info-box'>
                        <h4>📋 Información Importante</h4>
                        <p>Sirvase a acceder a nuestra plataforma para revisar los detalles de su solicitud y realizar las acciones correspondientes.</p>
                    </div>" : "")}
                    
                    {(esRechazo ? "<div style='text-align: center;'>" + botonAccion + "</div>" : "")}
                    
                    <div class='divider'></div>
                    
                    <p style='color: #6c757d; font-size: 14px;'>
                        <strong>Plataforma:</strong> <a href='{plataformaUrl}' style='color: #681E5B;'>{plataformaUrl}</a>
                    </p>
                    
                    {informacionAdicional}
                </div>
                
                <div class='footer'>
                    <p><strong>CMP - Colegio Médico del Perú</strong></p>
                    <p>Av. 28 de Julio 776 Miraflores Lima 18</p>
                    <p>Telf.: Aló CMP: 01 641-9847 | Correo: consultas@cmp.org.pe</p>
                    <p>Horario de atención: de lunes a viernes de 8:30 am. a 5:30 pm.</p>
                    <p style='font-size: 12px; color: #adb5bd; margin-top: 15px;'>
                        Este es un mensaje automático, por favor no responda a este correo.
                    </p>
                </div>
            </div>
        </body>
        </html>
        ";
        }
    }
}
