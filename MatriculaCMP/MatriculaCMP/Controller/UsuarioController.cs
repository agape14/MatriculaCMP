using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MatriculaCMP.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MatriculaCMP.Controller
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuarioController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		public UsuarioController(ApplicationDbContext context)
		{

			_context = context;
		}

		[HttpGet("ConexionServidor"), Authorize]
		public async Task<ActionResult<string>> GetEjemplo()
		{
			return "Autorizado para usar endpoint";
		}


		[HttpGet("Datos"), Authorize(Roles = "Conductor,Admin,Usuario")]
		public async Task<ActionResult<List<Usuario>>> GetCuenta()
		{
			var lista = await _context.Usuarios.ToListAsync();
			return Ok(lista);
		}

		public static Usuario usuario = new Usuario();
		[HttpPost("Registrar")]
		public async Task<ActionResult<string>> CreateCuenta(UsuarioDTO objeto)
		{
			try
			{
				CreatePasswordHash(objeto.Password, out byte[] passwordHash, out byte[] passwordSalt);
				usuario.NombreUsuario = objeto.NombreUsuario;
				usuario.Correo = objeto.Correo;
				usuario.PasswordHash = passwordHash;
				usuario.PasswordSalt = passwordSalt;

				_context.Usuarios.Add(usuario);
				await _context.SaveChangesAsync();
				var respuesta = "Registrado Con Exito";

				return respuesta;
			}
			catch (Exception ex)
			{
				return BadRequest("Error dutante el registro");
			}

		}

        [HttpPost("Login")]
        public async Task<ActionResult<string>> InicioSesion(UsuarioDTO objeto)
        {
            // Validación de campos de entrada
            if (string.IsNullOrWhiteSpace(objeto.Correo))
                return BadRequest("El correo electrónico es requerido.");

            if (string.IsNullOrWhiteSpace(objeto.Password))
                return BadRequest("La contraseña es requerida.");

            try
            {
                // Buscar usuario
                var cuenta = await _context.Usuarios.FirstOrDefaultAsync(x => x.Correo == objeto.Correo);
                if (cuenta == null)
                    return Unauthorized("Credenciales inválidas.");

                // Validación detallada de campos nulos
                if (cuenta.PasswordHash == null && cuenta.PasswordSalt == null)
                    return StatusCode(500, "Error de configuración: tanto PasswordHash como PasswordSalt son nulos.");

                if (cuenta.PasswordHash == null)
                    return StatusCode(500, "Error de configuración: PasswordHash es nulo.");

                if (cuenta.PasswordSalt == null)
                    return StatusCode(500, "Error de configuración: PasswordSalt es nulo.");

                // Verificar contraseña
                if (!VerifyPasswordHash(objeto.Password, cuenta.PasswordHash, cuenta.PasswordSalt))
                    return Unauthorized("Credenciales inválidas.");

                // Generar token
                return Ok(CreateToken(cuenta));
            }
            catch (Exception ex)
            {
                // Loggear el error (recomendado)
                // _logger.LogError(ex, "Error en el proceso de login");

                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
			using (var hmac = new HMACSHA512())
			{
				passwordSalt = hmac.Key;
				passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
			}
		}

		private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
		{
			using (var hmac = new HMACSHA512(passwordSalt))
			{
				var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
				return computedHash.SequenceEqual(passwordHash);
			}
		}


		private string CreateToken(Usuario user)
		{
			List<Claim> claims = new List<Claim>
 			{
				 new Claim(ClaimTypes.Name, user.Correo),
				 new Claim(ClaimTypes.Role,user.Rol),
                 new Claim(ClaimTypes.Name, user.NombreUsuario),
				new Claim("PerfilId", user.PerfilId.ToString()),
             };

			//var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
			//	"PROYECTO CONTROL USUARUIS EN BLAZOR WEB WASM_ DAGO PARA BLAZOR  PARA APPS"));
			
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				"PROYECTO COLEGIO MEDICO DEL PERU EN BLAZOR WEB WASM_ DAGO PARA BLAZOR FOR AGAPE_DEV"));

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var token = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.Now.AddMinutes(1),
				signingCredentials: creds);

			var jwt = new JwtSecurityTokenHandler().WriteToken(token);

			return jwt;
		}


	}
}
