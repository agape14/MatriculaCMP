using Microsoft.AspNetCore.Mvc;

namespace MatriculaCMP.Controller
{
	[ApiController]
	[Route("api/[controller]")]
	public class ConfigController : ControllerBase
	{
		private readonly IConfiguration _config;

		public ConfigController(IConfiguration config)
		{
			_config = config;
		}

		/// <summary>
		/// Devuelve los requisitos previos para prematrícula desde AppSettings.
		/// Permite modificar el texto en appsettings.json sin recompilar.
		/// </summary>
		[HttpGet("prematricula-requisitos")]
		public IActionResult GetPrematriculaRequisitos()
		{
			var section = _config.GetSection("Prematricula:RequisitosPrevios");
			var titulo = section["Titulo"] ?? "Requisitos previos para el registro";
			var confirmar = section["Confirmar"] ?? "Entendido";
			var encabezado = section["Encabezado"] ?? "Antes de iniciar el registro, asegúrese de tener:";
			var notaFinal = section["NotaFinal"] ?? "Esta información es solo de carácter informativo.";
			var items = section.GetSection("Items").Get<string[]>() ?? Array.Empty<string>();

			var itemsHtml = string.Join("", items.Select(item =>
			{
				var (label, text) = ObtenerLabelYTexto(item);
				return string.IsNullOrEmpty(text)
					? $"<li>• {label}</li>"
					: $"<li>• <strong>{label}:</strong> {text}</li>";
			}));

			var html = $@"
<div class=""text-start"">
	<p class=""mb-2""><strong>{EscapeHtml(encabezado)}</strong></p>
	<ul class=""list-unstyled"">
		{itemsHtml}
	</ul>
	<p class=""text-muted small mt-2 mb-0"">{EscapeHtml(notaFinal)}</p>
</div>";

			var mostrarCalloutEsMedicoContinuo = _config.GetValue<bool>("Prematricula:MostrarCalloutEsMedicoContinuo", false);
			return Ok(new { titulo, html, confirmar, mostrarCalloutEsMedicoContinuo });
		}

		private static (string Label, string Text) ObtenerLabelYTexto(string item)
		{
			var idx = item.IndexOf(": ", StringComparison.Ordinal);
			if (idx <= 0) return (EscapeHtml(item), "");
			return (EscapeHtml(item[..idx].Trim()), EscapeHtml(item[(idx + 2)..].Trim()));
		}

		private static string EscapeHtml(string? s)
		{
			if (string.IsNullOrEmpty(s)) return "";
			return s
				.Replace("&", "&amp;", StringComparison.Ordinal)
				.Replace("<", "&lt;", StringComparison.Ordinal)
				.Replace(">", "&gt;", StringComparison.Ordinal)
				.Replace("\"", "&quot;", StringComparison.Ordinal)
				.Replace("'", "&#39;", StringComparison.Ordinal);
		}
	}
}
