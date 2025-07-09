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
                var cuenta = await _context.Usuarios
                    .Include(u => u.Persona)
                    .Include(u => u.Perfil)
                    .FirstOrDefaultAsync(x => x.Correo == objeto.Correo);
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Para userId
                new Claim("Correo", user.Correo),
                new Claim("Usuario", user.NombreUsuario),
                new Claim("PersonaId", user.PersonaId.ToString()),
                new Claim("PerfilId", user.PerfilId.ToString()),
                new Claim("PerfilNombre", user.Perfil.Nombre),
                new Claim("NombresCompletos", user.Persona.NombresCompletos),
             };

			//var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
			//	"PROYECTO CONTROL USUARUIS EN BLAZOR WEB WASM_ DAGO PARA BLAZOR  PARA APPS"));
			
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				"PROYECTO COLEGIO MEDICO DEL PERU EN BLAZOR WEB WASM_ DAGO PARA BLAZOR FOR AGAPE_DEV"));

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],         // ✅ Emisor del token
                audience: _config["Jwt:Audience"],     // ✅ Destinatario válido
                claims: claims,
				expires: DateTime.Now.AddMinutes(1),
				signingCredentials: creds);

			var jwt = new JwtSecurityTokenHandler().WriteToken(token);

			return jwt;
		}

        [HttpPost("LoginSgd")]
        public async Task<IActionResult> LoginSgd([FromBody] UsuarioLoginEncryptedDTO request)
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
                var token="";
                var idPersona = 0;
                var persona_prem = _context.Personas.FirstOrDefault(x => x.NumeroDocumento == numDoc);
               
                if (persona_prem == null )
                {
                    if (persona_prem == null)
                    {
                        var nuevaPersona = new Persona
                        {
                            Nombres = persona.Nombres,
                            ApellidoPaterno = persona.ApellidoPaterno,
                            ApellidoMaterno = persona.ApellidoMaterno,
                            TipoDocumentoId = persona.IdCatalogoTipoDocumentoPersonal.ToString(),
                            NumeroDocumento = persona.NumeroDocumento
                        };

                        _context.Personas.Add(nuevaPersona);
                        await _context.SaveChangesAsync();
                        idPersona = nuevaPersona.Id;
                    }
                    else
                    {
                        idPersona = persona_prem.Id;
                    }
                    var usuario_prem = _sgdContext.UsuariosSGD.FirstOrDefault(x => x.IdPersona == persona.IdPersona && x.Bloqueado == false);
                    if (usuario_prem != null)
                    {
                        // Crear hash + salt (si quieres clave genérica)
                        var password = numDoc; // ⚠️ Opcional: puedes generar o encriptar una
                        CreatePasswordHash(password, out byte[] hash, out byte[] salt);

                        var nuevoUsuario = new Usuario
                        {
                            Correo = usuario_prem.Email ?? "", // o cualquier otro valor por defecto
                            NombreUsuario = usuario_prem.Logueo, // o algo único
                            PasswordHash = hash,
                            PasswordSalt = salt,
                            PerfilId = 2, // Medico
                            PersonaId = idPersona
                        };

                        _context.Usuarios.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();
                        var usuarioCompleto = _context.Usuarios
                        .Include(u => u.Persona)
                        .Include(u => u.Perfil)
                        .FirstOrDefault(u => u.Id == nuevoUsuario.Id);

                        token = CreateToken(usuarioCompleto);
                    }

                }
                else
                {
                    var usuariotoken = _context.Usuarios
                       .Include(u => u.Persona)
                       .Include(u => u.Perfil)
                       .FirstOrDefault(u => u.PersonaId == persona_prem.Id);
                    token = CreateToken(usuariotoken);
                }
                // Generar token de autenticación (ejemplo simplificado)
                //var token = GenerateJwtToken(persona);

                // Redirigir a la aplicación con el token de autenticación
                //return Redirect($"{_config["FrontendUrl"]}/autologin?token={token}");
                return Ok(new { token });
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
        public async Task<IActionResult> LoginSgdRedirect([FromQuery] string tipo, [FromQuery] string numero)
        {
            var dto = new UsuarioLoginEncryptedDTO
            {
                TipoDocumentoEncrypted = tipo,
                NumeroDocumentoEncrypted = numero
            };

            // Llama al método POST original
            return await LoginSgd(dto);
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
                    ValidateIssuer = false,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = false,
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
                              .Include(u => u.Persona)
                              .Select(u => new {
                                  u.Id,
                                  u.NombreUsuario,
                                  u.Correo,
                                  Persona = new { u.Persona.Id, u.Persona.Nombres, u.Persona.ApellidoPaterno, u.Persona.ApellidoMaterno, u.Persona.NombresCompletos },
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
                    var usuario = await _context.Usuarios
                .Include(x => x.Perfil)
                .Include(x => x.Persona)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

                if (usuario == null)
                    return NotFound();

                var model = new UsuarioInsert
                {
                    Nombres = usuario.Persona?.Nombres ?? "",
                    ApellidoPaterno = usuario.Persona?.ApellidoPaterno ?? "",
                    ApellidoMaterno = usuario.Persona?.ApellidoMaterno ?? "",
                    NumeroDocumento = usuario.Persona?.NumeroDocumento ?? "",
                    Correo = usuario.Correo,
                    NombreUsuario = usuario.NombreUsuario,
                    PerfilId = usuario.PerfilId,
                    Perfil = usuario.Perfil,
                    PersonaId = usuario.PersonaId,
                    Persona = usuario.Persona
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario {Id}", id);
                return StatusCode(500, "Ocurrió un error al consultar el usuario");
            }
        }

        // POST api/usuarios
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioInsert model)
        {
            try
            {
                // Mapear de UsuarioInsert → Usuario + Persona
                var persona = new Persona
                {
                    Nombres = model.Nombres.ToUpper().Trim(),
                    ApellidoPaterno = model.ApellidoPaterno.ToUpper().Trim(),
                    ApellidoMaterno = model.ApellidoMaterno.ToUpper().Trim(),
                    NumeroDocumento = model.NumeroDocumento,
                    Email = model.Correo,
                };

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var usuario = new Usuario
                {
                    Correo = model.Correo,
                    NombreUsuario = model.NombreUsuario.ToUpper().Trim(),
                    PerfilId = model.PerfilId,
                    PersonaId = persona.Id,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };

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
        public async Task<IActionResult> Put(int id, [FromBody] UsuarioInsert model)
        {
            if (id == null) return BadRequest("Id del cuerpo y de la URL no coinciden");

            try
            {
                var usuario = await _context.Usuarios
           .Include(u => u.Persona)
           .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                    return NotFound();

                // Actualizar datos de Persona
                if (usuario.Persona != null)
                {
                    usuario.Persona.Nombres = model.Nombres.ToUpper().Trim();
                    usuario.Persona.ApellidoPaterno = model.ApellidoPaterno.ToUpper().Trim();
                    usuario.Persona.ApellidoMaterno = model.ApellidoMaterno.ToUpper().Trim();
                    usuario.Persona.NumeroDocumento = model.NumeroDocumento;
                    usuario.Persona.Email = model.Correo;
                }

                // Actualizar datos de Usuario
                usuario.Correo = model.Correo;
                usuario.NombreUsuario = model.NombreUsuario.ToUpper().Trim();
                usuario.PerfilId = model.PerfilId;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    usuario.PasswordHash = passwordHash;
                    usuario.PasswordSalt = passwordSalt;
                }

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
