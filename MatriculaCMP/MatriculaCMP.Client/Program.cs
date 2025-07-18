using MatriculaCMP.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MatriculaCMP.Client;
using static System.Formats.Asn1.AsnWriter;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });//AGREGADO
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();//Agregado
//builder.Services.AddScoped<IMatriculaService, MatriculaHttpService>();
//builder.Services.AddScoped<MenuHttpService>();
builder.Services.AddAuthorizationCore();//Agregado
builder.Services.AddCascadingAuthenticationState();//Agregado
//builder.Services.AddScoped<PaisUniversidadesService>();

var host = builder.Build();
var scope = host.Services.CreateScope();
await builder.Build().RunAsync();
