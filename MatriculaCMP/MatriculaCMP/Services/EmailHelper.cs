using System.Net.Mail;
using System.Net;

namespace MatriculaCMP.Services
{
    public class EmailHelper
    {
        public static async Task EnviarCorreoCambioEstadoAsync(string destinatario, string nombre, string apellido, string estado)
        {
            string asunto = $"Estado de solicitud - {estado.ToUpper()}";
            string contenidoHtml = GetPlantillaHtml(nombre, apellido, estado);

            using var message = new MailMessage();
            message.To.Add(new MailAddress(destinatario));
            message.From = new MailAddress("ti05@cmp.org.pe", "CMP - Colegio Médico del Perú");
            message.Subject = asunto;
            message.Body = contenidoHtml;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient("smtp.office365.com", 587)
            {
                Credentials = new NetworkCredential("ti05@cmp.org.pe", "Agape1426**"),
                EnableSsl = true // Importante: Office365 requiere SSL
            };

            await smtp.SendMailAsync(message);
        }

        private static string GetPlantillaHtml(string nombre, string apellido, string estado)
        {
            return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background-color: #EDF2F6;
                    margin: 0;
                    padding: 0;
                }}
                .email-container {{ padding: 40px; }}
                table {{ width: 100%; background: white; border-radius: 10px; padding: 20px; }}
                .email-button {{
                    background-color: #3d78a8;
                    color: white;
                    padding: 10px 20px;
                    text-decoration: none;
                    border-radius: 5px;
                }}
                .email-footer {{ font-size: 12px; color: #777; text-align: center; margin-top: 30px; }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div style='text-align:center;'>
                    <img src='https://cmpchiclayo.org.pe/sistema/archivos/ckfinder/userfiles/images/logo.png' alt='Logo CMP' width='200'>
                </div>

                <table>
                    <tr>
                        <td>
                            <h2>Hola {nombre} {apellido}</h2>
                            <p>El estado de tu solicitud ha cambiado a: <strong>{estado}</strong>.</p>
                        </td>
                    </tr>
                </table>

                <div class='email-footer'>
                    Saludos,<br><strong>CMP - Colegio Médico del Perú</strong>
                </div>
            </div>
        </body>
        </html>
        ";
        }
    }
}
