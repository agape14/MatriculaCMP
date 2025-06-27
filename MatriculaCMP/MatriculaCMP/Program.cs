using MatriculaCMP.Client.Pages;
using MatriculaCMP.Components;
using MatriculaCMP.Data;
using MatriculaCMP.Interfaces;
using MatriculaCMP.Server.Data;
using MatriculaCMP.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
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
            ValidAudience = builder.Configuration["jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });//agregado
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
builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<UniversidadScraper>();
builder.Services.AddScoped<PaisesService>();
builder.Services.AddScoped<UniversidadesService>();
builder.Services.AddScoped<UniversidadScraper>();
builder.Services.AddScoped<MatriculaService>();
builder.Services.AddScoped<FirmaDigitalService>();
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
