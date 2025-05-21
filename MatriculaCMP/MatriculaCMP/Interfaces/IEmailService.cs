using MatriculaCMP.Shared;

namespace MatriculaCMP.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailRequest request);
        bool VerificarCodigo(string email, string codigoIngresad);
    }
}
