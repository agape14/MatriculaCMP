using MatriculaCMP.Interfaces;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MatriculaCMP.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(EmailRequest request)
        {
            await _emailService.SendEmailAsync(request);
            return Ok(new { message = "Correo enviado." });
        }

        [HttpPost("verificar")]
        public IActionResult VerificarCodigo([FromBody] VerificacionRequest model)
        {
            bool valido = _emailService.VerificarCodigo(model.Email, model.Codigo);
            if (!valido)
                return BadRequest(new { message = "Código inválido o expirado." });

            return Ok(new { message = "Código verificado correctamente." });
        }
    }
}
