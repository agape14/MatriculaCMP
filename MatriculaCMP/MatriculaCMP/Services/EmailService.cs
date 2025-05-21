using MatriculaCMP.Interfaces;
using MatriculaCMP.Shared;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace MatriculaCMP.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration config, IMemoryCache cache, IWebHostEnvironment env)
        {
            _config = config;
            _cache = cache;
            _env = env;
        }

        public async Task SendEmailAsync(EmailRequest request)
        {
            // Generar código
            var codigo = new Random().Next(100000, 999999).ToString();

            // Leer HTML y reemplazar código
            var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "VerificationTemplate.html");
            var htmlBody = await File.ReadAllTextAsync(templatePath);
            htmlBody = htmlBody.Replace("{{Codigo}}", codigo);

            // Configurar SMTP
            var host = _config["SmtpSettings:Host"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var user = _config["SmtpSettings:User"];
            var password = _config["SmtpSettings:Password"];
            var enableSsl = bool.Parse(_config["SmtpSettings:EnableSsl"]);

            using var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = enableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(user),
                Subject = request.Subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(request.To);

            await smtpClient.SendMailAsync(mail);

            // Guardar código en cache con 5 min de duración
            _cache.Set($"codigo:{request.To}", codigo, TimeSpan.FromMinutes(5));
        }

        public bool VerificarCodigo(string email, string codigoIngresado)
        {
            if (_cache.TryGetValue($"codigo:{email}", out string codigoGuardado))
            {
                return codigoGuardado == codigoIngresado;
            }
            return false;
        }

    }
}
