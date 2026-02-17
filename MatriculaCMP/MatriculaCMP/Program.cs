using MatriculaCMP.Client.Pages;
using MatriculaCMP.Components;
using MatriculaCMP.Data;
using MatriculaCMP.Interfaces;
using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddAuthorizationCore();//Agregado
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();//Agregado
builder.Services.AddControllers();//agregado
builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            o.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;
        });
//builder.Services.AddHttpClient();//Agregado
// Para IHttpClientFactory (necesario para FotoValidatorController)
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["FrontendUrl"]) // ← pon aquí la URL de tu API
});

builder.Services.AddCascadingAuthenticationState();//Agregado
builder.Services.AddScoped<MenuService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();


//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		sqlServerOptions =>
		{
			sqlServerOptions.CommandTimeout(600); // ⏱️ tiempo en segundos (ej. 180 = 3 minutos)
		});
});

builder.Services.AddDbContext<SgdDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SGDConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(600); // 10 minutos
        });
});
builder.Services.AddMvc();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    })//agregado
	// Cookie para sesión local tras OIDC
	.AddCookie("Cookies", options =>
	{
		options.Cookie.Name = ".MatriculaCMP.Auth";
		options.LoginPath = "/api/usuario/IdPeru/Login";
		options.LogoutPath = "/api/usuario/IdPeru/Logout";
		options.SlidingExpiration = true;
		options.ExpireTimeSpan = TimeSpan.FromHours(8);
		
		// IMPORTANTE: SameSite=None es necesario para recibir el POST cross-site de RENIEC/IDPeru
		// El callback de ID Perú viene como form POST desde idaas.reniec.gob.pe
		options.Cookie.SameSite = SameSiteMode.None;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requerido cuando SameSite=None
	})
	// OpenID Connect con ID PERU
	.AddOpenIdConnect("IDPeru", options =>
	{
		options.SignInScheme = "Cookies";
		options.Authority = builder.Configuration["IDPeru:Authority"];
		options.ClientId = builder.Configuration["IDPeru:ClientId"];
		options.ClientSecret = builder.Configuration["IDPeru:ClientSecret"];
		options.CallbackPath = builder.Configuration["IDPeru:CallbackPath"] ?? "/signin-idperu";
		options.ResponseType = OpenIdConnectResponseType.Code;
		options.SaveTokens = true;
		
		// CAMBIO CRÍTICO: Usar 'query' en lugar de 'form_post' 
		// El form_post de RENIEC no llega al servidor (bloqueado por browser/cookies)
		// Con 'query', el código viene en la URL y evita problemas cross-site
		options.ResponseMode = OpenIdConnectResponseMode.Query;
		
		// Desactivar PKCE temporalmente para debugging
		// (RENIEC puede tener problemas con code_challenge)
		options.UsePkce = false;

		options.GetClaimsFromUserInfoEndpoint = true;
		options.Scope.Clear();
		options.Scope.Add("openid");
		options.Scope.Add("profile");

		// Mapeo de claims personalizados
		options.ClaimActions.MapJsonKey("doc", "doc");
		options.ClaimActions.MapJsonKey("first_name", "first_name");
		options.ClaimActions.MapJsonKey("email", "email");
		
		// IMPORTANTE: Configurar cookies de correlación para cross-site de RENIEC
		options.NonceCookie.SameSite = SameSiteMode.None;
		options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
		options.CorrelationCookie.SameSite = SameSiteMode.None;
		options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

		// Función helper para escribir logs a archivo (disponible antes del Build)
		static void LogToFile(string level, string message)
		{
			try
			{
				var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
				if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
				var logFile = Path.Combine(logDir, $"idperu-{DateTime.Now:yyyy-MM-dd}.log");
				var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
				File.AppendAllText(logFile, logLine);
			}
			catch { /* No romper si falla el logging */ }
		}
		
		options.Events = new OpenIdConnectEvents
		{
			OnRedirectToIdentityProvider = ctx =>
			{
				var acr = builder.Configuration["IDPeru:Acr"];
				if (!string.IsNullOrWhiteSpace(acr))
				{
					ctx.ProtocolMessage.SetParameter("acr_values", acr);
				}
				
				// Obtener DNI: 1) HttpContext.Items (misma petición, set por DDJJ antes de Challenge)
				// 2) AuthenticationProperties, 3) cookie (petición previa)
				string? dni = null;
				if (ctx.HttpContext.Items.TryGetValue("IDPeru_DNI", out var dniFromItems) && dniFromItems is string dniStr && 
				    !string.IsNullOrEmpty(dniStr) && dniStr.Length == 8 && dniStr.All(char.IsDigit))
				{
					dni = dniStr;
				}
				else if (ctx.Properties?.Items?.TryGetValue("dni", out var dniFromProps) == true && !string.IsNullOrEmpty(dniFromProps))
				{
					dni = dniFromProps;
				}
				else if (ctx.HttpContext.Request.Cookies.TryGetValue("IDPeru_DNI", out var dniCookie) && 
				         !string.IsNullOrEmpty(dniCookie) && dniCookie.Length == 8 && dniCookie.All(char.IsDigit))
				{
					dni = dniCookie;
				}
				
				// Agregar vd via SetParameter: el middleware incluirá vd en la URL y agregará state después
				// (redirect_uri y state con URL encoding según manual - el middleware los aplica)
				if (!string.IsNullOrEmpty(dni))
				{
					try
					{
						var clientId = builder.Configuration["IDPeru:ClientId"] ?? "";
						if (!string.IsNullOrEmpty(clientId) && clientId.Length >= 16)
						{
							// urlEncode: false - el middleware OIDC aplica encoding al construir la URL
							var vdValor = IdPeruUrlService.EncriptarDniParaVd(dni, clientId, urlEncode: false);
							ctx.ProtocolMessage.SetParameter("vd", vdValor);
							LogToFile("INFO", $"[IDPeru] Parámetro vd agregado (DNI encriptado). DNI: {dni}");
							var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
							logger.LogInformation($"[IDPeru] Parámetro vd agregado a URL ID Perú");
						}
					}
					catch (Exception ex)
					{
						LogToFile("ERROR", $"[IDPeru] Error al encriptar DNI para vd: {ex.Message}");
						var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogError(ex, "[IDPeru] Error al encriptar DNI para parámetro vd");
					}
				}
				else
				{
					LogToFile("WARN", "[IDPeru] No se encontró DNI. No se agregará parámetro 'vd'.");
				}
				
				return Task.CompletedTask;
			},
			
			// Capturar cuando llega el código de autorización (ANTES de intercambiarlo por token)
			OnAuthorizationCodeReceived = ctx =>
			{
				var codePreview = ctx.ProtocolMessage?.Code?.Substring(0, Math.Min(10, ctx.ProtocolMessage?.Code?.Length ?? 0)) + "...";
				var msg = $"[IDPeru] ✅ Código de autorización RECIBIDO. Code: {codePreview}";
				LogToFile("INFO", msg);
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
				logger.LogInformation(msg);
				return Task.CompletedTask;
			},
			
			// Capturar cuando se obtienen los tokens exitosamente
			OnTokenResponseReceived = ctx =>
			{
				var msg = "[IDPeru] ✅ Token RECIBIDO exitosamente";
				LogToFile("INFO", msg);
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
				logger.LogInformation(msg);
				return Task.CompletedTask;
			},
			
			// Capturar cuando la autenticación es exitosa
			OnTicketReceived = ctx =>
			{
				var claims = ctx.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
				var claimsStr = string.Join(", ", claims ?? new List<string>());
				var msg = $"[IDPeru] ✅ Ticket RECIBIDO. Claims: {claimsStr}";
				LogToFile("INFO", msg);
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
				logger.LogInformation(msg);
				return Task.CompletedTask;
			},
			
			// Manejar errores cuando el usuario cancela o hay un fallo en la autenticación
			OnRemoteFailure = ctx =>
			{
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
				
				// Limpiar cookies en caso de error
				ctx.Response.Cookies.Delete("IDPeru_DNI");
				ctx.Response.Cookies.Delete("IDPeru_SolicitudId");
				
				// Log detallado del error
				var errorFromQuery = ctx.Request.Query["error"].ToString();
				var errorDescription = ctx.Request.Query["error_description"].ToString();
				var failureMessage = ctx.Failure?.Message ?? "Sin mensaje";
				
				var errorMsg = $"[IDPeru] ❌ Remote Failure. Error: {errorFromQuery}, Description: {errorDescription}, Exception: {failureMessage}";
				LogToFile("ERROR", errorMsg);
				logger.LogWarning(errorMsg);
				
				// Intentar obtener el returnUrl de la cookie guardada en DDJJController
				var returnUrl = "/solicitudes/prematricula";
				
				if (ctx.Request.Cookies.TryGetValue("IDPeru_ReturnUrl", out var cookieReturnUrl) && 
					!string.IsNullOrEmpty(cookieReturnUrl))
				{
					returnUrl = cookieReturnUrl;
					// Eliminar la cookie después de usarla
					ctx.Response.Cookies.Delete("IDPeru_ReturnUrl");
				}
				
				var separator = returnUrl.Contains("?") ? "&" : "?";
				
				// Si el error es "user_cancelled" pero NO viene de la query original, 
				// verificar si hay un código válido que se pueda procesar
				if (errorFromQuery == "user_cancelled")
				{
					LogToFile("WARN", $"[IDPeru] Usuario canceló o timeout de página. Redirigiendo a: {returnUrl}");
					ctx.Response.Redirect($"{returnUrl}{separator}ddjj=cancelled");
				}
				else if (!string.IsNullOrEmpty(errorFromQuery))
				{
					ctx.Response.Redirect($"{returnUrl}{separator}ddjj=error&msg={Uri.EscapeDataString(errorFromQuery)}&desc={Uri.EscapeDataString(errorDescription)}");
				}
				else if (ctx.Failure != null)
				{
					// Si hay una excepción, loguear más detalles
					LogToFile("ERROR", $"[IDPeru] Excepción: {ctx.Failure.Message}\n{ctx.Failure.StackTrace}");
					logger.LogError(ctx.Failure, "[IDPeru] Excepción durante autenticación");
					ctx.Response.Redirect($"{returnUrl}{separator}ddjj=error&msg={Uri.EscapeDataString(ctx.Failure.Message)}");
				}
				else
				{
					ctx.Response.Redirect($"{returnUrl}{separator}ddjj=error");
				}
				
				ctx.HandleResponse(); // Evitar que se muestre la página de error por defecto
				return Task.CompletedTask;
			},
			
			// Capturar errores de validación de mensaje (state, nonce, etc.)
			OnMessageReceived = ctx =>
			{
				var code = ctx.ProtocolMessage?.Code;
				var error = ctx.ProtocolMessage?.Error;
				var state = ctx.ProtocolMessage?.State;
				var statePreview = state?.Substring(0, Math.Min(20, state?.Length ?? 0)) + "...";
				
				var msg = $"[IDPeru] 📨 Mensaje RECIBIDO. HasCode: {!string.IsNullOrEmpty(code)}, HasError: {!string.IsNullOrEmpty(error)}, State: {statePreview}";
				LogToFile("INFO", msg);
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
				logger.LogInformation(msg);
				
				return Task.CompletedTask;
			}
		};

		// Configuración de validación de tokens con la clave pública de RENIEC
		// Clave obtenida de: https://idaas.reniec.gob.pe/service/certs (configurable en appsettings.json)
		var reniecPublicKey = builder.Configuration["IDPeru:SigningKey"];
		
		if (!string.IsNullOrEmpty(reniecPublicKey))
		{
			// Reemplazar \n literal por saltos de línea reales si vienen del JSON
			reniecPublicKey = reniecPublicKey.Replace("\\n", "\n");
			
			// Convertir la clave PEM a RSA SecurityKey
			var rsa = System.Security.Cryptography.RSA.Create();
			rsa.ImportFromPem(reniecPublicKey);
			var rsaSecurityKey = new RsaSecurityKey(rsa);
			
			options.TokenValidationParameters.IssuerSigningKey = rsaSecurityKey;
			options.TokenValidationParameters.ValidateIssuerSigningKey = true;
		}
		else
		{
			// Si no hay clave configurada, deshabilitar validación de firma (menos seguro)
			options.TokenValidationParameters.ValidateIssuerSigningKey = false;
			options.TokenValidationParameters.RequireSignedTokens = false;
		}
		
		options.TokenValidationParameters.NameClaimType = "doc";
		options.TokenValidationParameters.ValidateIssuer = false; // RENIEC puede tener issuer diferente
		options.TokenValidationParameters.ValidateAudience = false; // Flexibilizar para ID Perú
		options.TokenValidationParameters.ValidateLifetime = true;
	});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin() // o .WithOrigins("https://localhost:port") si prefieres restringir
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddSingleton<IdPeruLogService>(); // Logging a archivo para ID Perú
builder.Services.AddScoped<IdPeruUrlService>(); // Construcción de URL ID Perú con parámetro vd
builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<UniversidadScraper>();
builder.Services.AddScoped<PaisesService>();
builder.Services.AddScoped<UniversidadesService>();
builder.Services.AddScoped<UniversidadScraper>();
builder.Services.AddScoped<MatriculaService>();
builder.Services.AddScoped<FirmaDigitalService>();
builder.Services.AddScoped<FirmaDigitalLoteService>();
builder.Services.AddScoped<IConsultaEsMedicoService, ConsultaEsMedicoService>();
// Inicializar EmailHelper con la configuración
EmailHelper.Initialize(builder.Configuration);

var app = builder.Build();
app.UseCors("AllowAll"); // 🔴 Muy importante: debe estar antes de UseAuthorization
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();//Agregado
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MatriculaCMP.Client._Imports).Assembly);

app.Run();
