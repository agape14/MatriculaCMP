using MatriculaCMP.Client.Pages;
using MatriculaCMP.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using MatriculaCMP.Server.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MatriculaCMP.Services;
using MatriculaCMP.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddAuthorizationCore();//Agregado
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();//Agregado
builder.Services.AddControllers();//agregado
builder.Services.AddHttpClient();//Agregado
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

builder.Services.AddMvc();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
				.GetBytes("PROYECTO COLEGIO MEDICO DEL PERU EN BLAZOR WEB WASM_ DAGO PARA BLAZOR FOR AGAPE_DEV")),
			ValidateIssuer = false,
			ValidateAudience = false
		};
	});//agregado

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<UniversidadScraper>();
builder.Services.AddScoped<PaisesService>();
builder.Services.AddScoped<UniversidadesService>();
builder.Services.AddScoped<UniversidadScraper>();

var app = builder.Build();

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
