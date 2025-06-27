using MatriculaCMP.Data;
using MatriculaCMP.Server.Data;
using MatriculaCMP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
		private readonly SgdDbContext _sgdContext;
        private readonly IConfiguration _config;
        private readonly ILogger<UsuarioController> _logger;
        public UsuarioController(ApplicationDbContext context, SgdDbContext sgdContext, IConfiguration config,ILogger<UsuarioController> logger)
		{

			_context = context;
            _sgdContext = sgdContext;
            _config = config;
            _logger = logger;
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
                usuario.PerfilId = 1;
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
                _logger.LogError(ex, "Error en el proceso de login");
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
				 //new Claim(ClaimTypes.Role,user.Rol),
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


        //[HttpPost("LoginSgd")]
        //public async Task<IActionResult> LoginDesdeSgd([FromBody] UsuarioLoginEncryptedDTO dto, [FromServices] SgdDbContext sgdContext)
        //{
        //    string clave = "clave-secreta";

        //    try
        //    {
        //        string tipoDocumentoDesencriptado = Decrypt(dto.TipoDocumentoEncrypted, clave);
        //        string numeroDocumentoDesencriptado = Decrypt(dto.NumeroDocumentoEncrypted, clave);

        //        int tipoDoc = int.Parse(tipoDocumentoDesencriptado);
        //        string numDoc = numeroDocumentoDesencriptado;

        //        var persona = await sgdContext.Persona
        //            .FirstOrDefaultAsync(p => p.IdCatalogoTipoDocumentoPersonal == tipoDoc && p.NumeroDocumento == numDoc);

        //        if (persona == null)
        //        {
        //            return NotFound("No se encontró el usuario. Comuníquese con el administrador.");
        //        }

        //        return Ok(new
        //        {
        //            mensaje = "Login exitoso",
        //            nombre = persona.NombreCompleto
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Error interno: " + ex.Message);
        //    }
        //}


        [HttpPost("LoginSgd")]
        public IActionResult LoginSgd([FromBody] UsuarioLoginEncryptedDTO request)
        {
            try
            {
                // Desencriptar Base64
                string tipoDoc = DecryptBase64(request.TipoDocumentoEncrypted);
                string numDoc = DecryptBase64(request.NumeroDocumentoEncrypted);

                var persona = _sgdContext.Persona
                    .FirstOrDefault(p => p.IdCatalogoTipoDocumentoPersonal.ToString() == tipoDoc &&
                            p.NumeroDocumento == numDoc);

                if (persona == null)
                {
                    return NotFound("No se encontró el usuario. Comuníquese con el administrador.");
                }

                // Generar token de autenticación (ejemplo simplificado)
                var token = GenerateJwtToken(persona);

                // Redirigir a la aplicación con el token de autenticación
                return Redirect($"{_config["FrontendUrl"]}/autologin?token={token}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar la solicitud: {ex.Message}");
            }
        }

        private static string DecryptBase64(string base64String)
        {
            byte[] data = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(data);
        }


        [HttpPost("ProbarDecrypt")]
        public IActionResult ProbarDecrypt([FromBody] string base64)
        {
            try
            {
                var resultado = DecryptBase64(base64);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // En tu controlador API
        [HttpGet("LoginSgdRedirect")]
        public IActionResult LoginSgdRedirect([FromQuery] string tipo, [FromQuery] string numero)
        {
            var dto = new UsuarioLoginEncryptedDTO
            {
                TipoDocumentoEncrypted = tipo,
                NumeroDocumentoEncrypted = numero
            };

            // Llama al método POST original
            return LoginSgd(dto);
        }

        private string GenerateJwtToken(PersonaSGD persona)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, persona.NumeroDocumento),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("nombrescompletos", persona.NombreCompleto),
                new Claim("tipoDocumento", persona.IdCatalogoTipoDocumentoPersonal.ToString())
                // Agrega más claims según necesites
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("validate")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Ok(new { valid = true });
            }
            catch
            {
                return Unauthorized(new { valid = false });
            }
        }


        // GET api/usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> Get()
        {
            try
            {
                var lista = await _context.Usuarios
                              .Include(u => u.Perfil)
                              .Select(u => new {
                                  u.Id,
                                  u.NombreUsuario,
                                  u.Correo,
                                  Perfil = new { u.Perfil.Id, u.Perfil.Nombre }
                              })
                              .ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, "Ocurrió un error al consultar los usuarios");
            }
        }

        // GET api/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> Get(int id)
        {
            try
            {
                var u = await _context.Usuarios
                                      .Include(x => x.Perfil)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(x => x.Id == id);

                return u is null ? NotFound() : Ok(u);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario {Id}", id);
                return StatusCode(500, "Ocurrió un error al consultar el usuario");
            }
        }

        // POST api/usuarios
        [HttpPost]
        public async Task<IActionResult> Post(Usuario usuario)
        {
            try
            {
                // Si viene con datos de Persona
                if (usuario.Persona is not null)
                {
                    _context.Personas.Add(usuario.Persona);
                    await _context.SaveChangesAsync();

                    // Asignar el ID recién generado
                    usuario.PersonaId = usuario.Persona.Id;
                }
                CreatePasswordHash(usuario.Password, out byte[] passwordHash, out byte[] passwordSalt);
                // Preparar hash y salt (si aplica)
                usuario.PasswordSalt = passwordSalt;
                usuario.PasswordHash = passwordHash; // ejemplo fijo o reemplaza

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, "Ocurrió un error al crear el usuario");
            }
        }

        // PUT api/usuarios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Usuario dto)
        {
            if (id != dto.Id) return BadRequest("Id del cuerpo y de la URL no coinciden");

            try
            {
                _context.Entry(dto).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException cx)
            {
                _logger.LogWarning(cx, "Conflicto de concurrencia al actualizar {Id}", id);
                return NotFound("El usuario fue eliminado o modificado por otro proceso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar {Id}", id);
                return StatusCode(500, "Ocurrió un error al actualizar el usuario");
            }
        }

        // DELETE api/usuarios/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var u = await _context.Usuarios.FindAsync(id);
                if (u is null) return NotFound();

                _context.Usuarios.Remove(u);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar {Id}", id);
                return StatusCode(500, "Ocurrió un error al eliminar el usuario");
            }
        }
    }
}
