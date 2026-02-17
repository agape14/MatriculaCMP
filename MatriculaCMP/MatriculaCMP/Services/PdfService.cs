using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using MatriculaCMP.Shared;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text;

namespace MatriculaCMP.Services
{
    public class PdfLayoutOptions
    {
        public float WatermarkOpacity { get; set; } = 0.18f;
        public float WatermarkScale { get; set; } = 0.6f; // porcentaje relativo del tamaño de página
        public float BottomReservedHeight { get; set; } = 90f; // espacio para sello/leyenda de firma digital
        public float SignaturesTopSpacing { get; set; } = 6f;
        public float SignaturesRowSpacing { get; set; } = 4f;
        public float SignatureLineWidthPercent { get; set; } = 55f;
        public float SignatureTitleFontSize { get; set; } = 9f;
        public float SignatureSubFontSize { get; set; } = 8f;
        // Desfase vertical bajo el rectángulo de la firma (para que la firma quede por encima)
        public float SignatureLineOffsetBelowRect { get; set; } = 18f;
        public float SignatureTitleGap { get; set; } = 9f;
        public float SignatureSubGap { get; set; } = 8f;
    }

    public class PdfService
    {
        private readonly ILogger<PdfService>? _logger;
        private readonly PdfLayoutOptions _layout;
        private readonly Dictionary<string, (int Page, float X1, float Y1, float X2, float Y2)> _signatureRects = new Dictionary<string, (int, float, float, float, float)>();
        private readonly IConfiguration? _configuration;

        public PdfService()
        {
            _layout = new PdfLayoutOptions();
            try
            {
                // Cargar configuración local si no hay DI
                string? cfgPath = ResolveAssetPath("appsettings.json");
                if (!string.IsNullOrEmpty(cfgPath) && File.Exists(cfgPath))
                {
                    var cfg = new ConfigurationBuilder()
                        .AddJsonFile(cfgPath, optional: true, reloadOnChange: false)
                        .Build();
                    _configuration = cfg;
                    cfg.GetSection("PdfLayout").Bind(_layout);
                    LoadSignatureRects(cfg);
                }
            }
            catch
            {
                // Ignorar errores de configuración en ctor por defecto
            }
        }

        public PdfService(ILogger<PdfService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _layout = new PdfLayoutOptions();
            _configuration = configuration;
            try
            {
                configuration.GetSection("PdfLayout").Bind(_layout);
                LoadSignatureRects(configuration);
            }
            catch
            {
                // Si no hay configuración, se mantienen los valores por defecto
            }
        }

        public byte[] GenerarDiplomaPdf(Diploma diploma)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Reducir aún más el margen superior para acercar el contenido al borde superior
                float bottomMargin = 20f + _layout.BottomReservedHeight; // reservar espacio para sellos/leyendas de firma digital
                Document document = new Document(PageSize.A4, 50, 50, 10, bottomMargin);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Configurar fuentes
                // Intentar cargar "Old English Text MT" (archivo común: OLDENGL.TTF). Si no existe, usar Helvetica.
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                BaseFont oldEnglishBase;
                try
                {
                    // 1) Buscar primero en wwwroot/fonts (fuente incrustada con la app)
                    var appFontsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "fonts");
                    var candidates = new[] { "OldEnglishTextMT.ttf", "OLDENGL.TTF", "OldeEnglish.ttf", "OLDENGLB.TTF" };
                    string? found = candidates
                        .Select(c => Path.Combine(appFontsDir, c))
                        .FirstOrDefault(File.Exists);

                    // 2) Si no está incrustada, buscar en la carpeta de fuentes del sistema
                    if (string.IsNullOrEmpty(found))
                    {
                        var systemFontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                        found = candidates
                            .Select(c => Path.Combine(systemFontsPath, c))
                            .FirstOrDefault(File.Exists);
                    }

                    oldEnglishBase = !string.IsNullOrEmpty(found)
                        ? BaseFont.CreateFont(found, BaseFont.CP1252, BaseFont.EMBEDDED)
                        : baseFont;
                }
                catch
                {
                    oldEnglishBase = baseFont;
                }

                // Fuentes finales
                // Fijo: Old English Text MT (sin negrita para imitar el documento de referencia)
                // Reducir un poco el título para intentar mantenerlo en una sola línea
                Font titleBigFont = new Font(oldEnglishBase, 48, Font.NORMAL);
                Font titleSmallFont = new Font(oldEnglishBase, 32, Font.NORMAL);
                Font fixedFont26 = new Font(oldEnglishBase, 24, Font.NORMAL);
                Font fixedFont18 = new Font(oldEnglishBase, 18, Font.NORMAL);
                // Dinámico: Helvetica (o fuente estándar) para que sea más legible en una sola línea
                Font dynamicFont26 = new Font(baseFont, 26, Font.BOLD);
                Font dynamicFont22 = new Font(baseFont, 22, Font.BOLD);
                Font dynamicFont18 = new Font(baseFont, 18, Font.BOLD);

                // Marca de agua (centrada y translúcida)
                try
                {
                    string? watermarkPath = ResolveAssetPath("wwwroot", "assets", "images", "logotipo-cmp.png");
                    _logger?.LogInformation("Aplicando marca de agua. Path: {Path}. Existe: {Exists}", watermarkPath, !string.IsNullOrEmpty(watermarkPath) && File.Exists(watermarkPath));
                    if (!string.IsNullOrEmpty(watermarkPath) && File.Exists(watermarkPath))
                    {
                        Image watermark = Image.GetInstance(watermarkPath);
                        float pageWidth = document.PageSize.Width;
                        float pageHeight = document.PageSize.Height;
                        // Escalar grande, estilo marca de agua
                        watermark.ScaleToFit(pageWidth * _layout.WatermarkScale, pageHeight * _layout.WatermarkScale);
                        float x = (pageWidth - watermark.ScaledWidth) / 2f;
                        float y = (pageHeight - watermark.ScaledHeight) / 2f;

                        PdfGState gState = new PdfGState { FillOpacity = _layout.WatermarkOpacity, StrokeOpacity = _layout.WatermarkOpacity };
                        PdfContentByte canvas = writer.DirectContentUnder;
                        canvas.SaveState();
                        canvas.SetGState(gState);
                        watermark.SetAbsolutePosition(x, y);
                        canvas.AddImage(watermark);
                        canvas.RestoreState();
                    }
                    else
                    {
                        _logger?.LogWarning("No se encontró la imagen de marca de agua. Intentos de resolución fallidos.");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error al aplicar la marca de agua");
                }

                // Título principal (en dos líneas: "El" y debajo el título)
                Paragraph elTitulo = new Paragraph(new Chunk("El", titleSmallFont))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = -12f
                };
                document.Add(elTitulo);

                Paragraph titulo = new Paragraph(new Chunk("Colegio Médico del Perú", titleBigFont))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = -6f,
                    SpacingAfter = 0f
                };
                document.Add(titulo);

                // Por cuanto
                Paragraph porCuanto = new Paragraph("Por cuanto, el médico cirujano", fixedFont26);
                porCuanto.Alignment = Element.ALIGN_LEFT;
                porCuanto.IndentationLeft = 40f;
                porCuanto.SpacingAfter = 1f;
                document.Add(porCuanto);

                // Nombre completo (dinámico)
                var nombreTexto = (diploma.NombreCompleto ?? string.Empty).ToUpper();
                float contenidoAncho = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                float tamNombre = 12f;
                while (baseFont.GetWidthPoint(nombreTexto, tamNombre) > contenidoAncho && tamNombre > 12f)
                {
                    tamNombre -= 0.5f;
                }
                var nombreFont = new Font(baseFont, tamNombre, Font.BOLD);
                Paragraph nombre = new Paragraph(nombreTexto, nombreFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 1f
                };
                document.Add(nombre);
                // Línea bajo el nombre
                var nombreLinea = new LineSeparator(0.8f, 70f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f);
                var nombreLineaPar = new Paragraph();
                nombreLineaPar.Add(new Chunk(nombreLinea));
                nombreLineaPar.SpacingAfter = 2f;
                document.Add(nombreLineaPar);

                // Graduado en
                Paragraph graduado = new Paragraph("Graduado en:", fixedFont26);
                graduado.Alignment = Element.ALIGN_LEFT;
                graduado.IndentationLeft = 40f;
                graduado.SpacingAfter = 2f;
                document.Add(graduado);

                // Universidad seleccionada
                var universidadNombre = string.IsNullOrWhiteSpace(diploma.UniversidadNombre) ? string.Empty : diploma.UniversidadNombre.ToUpper();
                float tamUni = 12f;
                while (baseFont.GetWidthPoint(universidadNombre, tamUni) > contenidoAncho && tamUni > 12f)
                {
                    tamUni -= 0.5f;
                }
                var uniFont = new Font(baseFont, tamUni, Font.BOLD);
                Paragraph especialidad = new Paragraph(universidadNombre, uniFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 1f
                };
                document.Add(especialidad);
                // Línea bajo universidad
                var uniLinea = new LineSeparator(0.8f, 80f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f);
                var uniLineaPar = new Paragraph();
                uniLineaPar.Add(new Chunk(uniLinea));
                uniLineaPar.SpacingAfter = 2f;
                document.Add(uniLineaPar);

                // Texto principal en 26, con número de colegiatura en fuente dinámica
                Paragraph mainText = new Paragraph ("ha cumplido con las disposiciones estatutarias y reglamentarias vigentes y está inscrito en el Registro Nacional de Matrículas, con el N° ",
                fixedFont26);
                
                // Seguir exactamente el formato/separación de finalText
                mainText.Alignment = Element.ALIGN_JUSTIFIED;
                var dynamicFont12 = new Font(baseFont, 12, Font.BOLD);
                mainText.Add(new Chunk(diploma.NumeroColegiatura ?? string.Empty, dynamicFont12));
                mainText.Add(new Chunk(".", fixedFont26));
                mainText.SetLeading(0, 1.1f);
                mainText.SpacingAfter = 1f;
                mainText.IndentationLeft = 40f; 
                mainText.IndentationRight = 40f;
                document.Add(mainText);

                // Por tanto
                Paragraph porTanto = new Paragraph("Por tanto,", fixedFont26);
                porTanto.Alignment = Element.ALIGN_LEFT;
                porTanto.IndentationLeft = 40f;
                porTanto.SpacingAfter = 1f;
                document.Add(porTanto);

                // Texto final
                Paragraph finalText = new Paragraph(
                    "se le expide el presente Certificado, que lo acredita como colegiado y lo faculta para el ejercicio de la profesión en el territorio de la República.",
                    fixedFont26);
                finalText.Alignment = Element.ALIGN_JUSTIFIED;
                // Interlineado moderado
                finalText.SetLeading(0, 1.1f);
                finalText.SpacingAfter = 1f;
                finalText.IndentationLeft = 40f;
                finalText.IndentationRight = 40f;
                document.Add(finalText);

                // Fecha y lugar (18)
                var fechaTexto = new Paragraph { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 6f };
                fechaTexto.IndentationLeft = 40f;
                fechaTexto.IndentationRight = 40f;
                fechaTexto.Add(new Chunk("Lima, ", fixedFont18));
                fechaTexto.Add(new Chunk(diploma.FechaEmision.ToString("dd"), fixedFont18));
                fechaTexto.Add(new Chunk(" de ", fixedFont18));
                fechaTexto.Add(new Chunk(GetMonthName(diploma.FechaEmision.Month), fixedFont18));
                fechaTexto.Add(new Chunk(" del ", fixedFont18));
                fechaTexto.Add(new Chunk(diploma.FechaEmision.Year.ToString(), fixedFont18));
                document.Add(fechaTexto);

                // Etiquetas de cargos posicionadas bajo cada rectángulo de firma (coordenadas desde appsettings.json)
                var signatureTitleFont = new Font(baseFont, Math.Max(7f, _layout.SignatureTitleFontSize - 1f), Font.BOLD);
                var signatureSubFont = new Font(baseFont, Math.Max(6f, _layout.SignatureSubFontSize - 1f), Font.NORMAL);
                DrawCargoIfConfigured(writer, signatureTitleFont, signatureSubFont, "CoordenadasFirmaDecano", "DECANO", "CONSEJO NACIONAL");
                DrawCargoIfConfigured(writer, signatureTitleFont, signatureSubFont, "CoordenadasFirmaSGSecretario", "SECRETARIO GENERAL", "CONSEJO NACIONAL");
                DrawCargoIfConfigured(writer, signatureTitleFont, signatureSubFont, "CoordenadasFirmaCRDecano", "DECANO", "CONSEJO REGIONAL");
                DrawCargoIfConfigured(writer, signatureTitleFont, signatureSubFont, "CoordenadasFirmaCRSecretario", "SECRETARIO GENERAL", "CONSEJO REGIONAL");

                // Nota: las etiquetas se dibujan en contenido directo, sin afectar el flujo ni generar página adicional

                // Pie de página con leyenda, link y QR
                try
                {
                    DibujarPieDePagina(writer, document, diploma.SolicitudId);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error al dibujar pie de página");
                }

                document.Close();
                return ms.ToArray();
            }
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "enero",
                2 => "febrero",
                3 => "marzo",
                4 => "abril",
                5 => "mayo",
                6 => "junio",
                7 => "julio",
                8 => "agosto",
                9 => "septiembre",
                10 => "octubre",
                11 => "noviembre",
                12 => "diciembre",
                _ => "enero"
            };
        }

        // Busca un archivo empezando desde BaseDirectory y subiendo directorios padres
        private string? ResolveAssetPath(params string[] relativeSegments)
        {
            try
            {
                string combined = Path.Combine(relativeSegments);
                var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 8 && current != null; i++)
                {
                    string candidate = Path.Combine(current.FullName, combined);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                    current = current.Parent;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error resolviendo ruta de asset");
            }
            return null;
        }

        private void LoadSignatureRects(IConfiguration configuration)
        {
            foreach (var key in new[] { "CoordenadasFirmaCRSecretario", "CoordenadasFirmaCRDecano", "CoordenadasFirmaSGSecretario", "CoordenadasFirmaDecano" })
            {
                var value = configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                var parsed = ParseRect(value);
                if (parsed.HasValue)
                {
                    _signatureRects[key] = parsed.Value;
                }
            }
        }

        private (int Page, float X1, float Y1, float X2, float Y2)? ParseRect(string input)
        {
            try
            {
                var parts = input.Split(',');
                if (parts.Length < 5) return null;
                int page = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                float x1 = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                float y1 = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                float x2 = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
                float y2 = float.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);
                return (page, x1, y1, x2, y2);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error parseando coordenadas de firma: {Input}", input);
                return null;
            }
        }

        private void DrawCargoIfConfigured(PdfWriter writer, Font titleFont, Font subFont, string key, string titulo, string subtitulo)
        {
            if (!_signatureRects.TryGetValue(key, out var rect))
            {
                _logger?.LogDebug("Coordenadas no configuradas para {Key}", key);
                return;
            }
            // Solo página 1 soportada por ahora
            if (rect.Page != 1)
            {
                _logger?.LogWarning("La coordenada {Key} apunta a la página {Page}, pero este diploma es de 1 página.", key, rect.Page);
            }

            float centerX = (rect.X1 + rect.X2) / 2f;
            float width = Math.Abs(rect.X2 - rect.X1);
            float lineWidth = width * (_layout.SignatureLineWidthPercent / 100f);
            // Subir la firma: la línea va más abajo del rectángulo, controlado por offset configurable
            float lineY = Math.Min(rect.Y1, rect.Y2) - _layout.SignatureLineOffsetBelowRect;
            float titleY = lineY - _layout.SignatureTitleGap;
            float subY = titleY - _layout.SignatureSubGap;

            var canvas = writer.DirectContent;
            // línea
            canvas.SaveState();
            canvas.SetLineWidth(0.7f);
            canvas.MoveTo(centerX - lineWidth / 2f, lineY);
            canvas.LineTo(centerX + lineWidth / 2f, lineY);
            canvas.Stroke();
            canvas.RestoreState();

            // textos centrados
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, new Phrase(titulo.ToUpperInvariant(), titleFont), centerX, titleY, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, new Phrase(subtitulo.ToUpperInvariant(), subFont), centerX, subY, 0);
        }

        private string ConstruirUrlVerificacion(int solicitudId)
        {
            // Priorizar URL explícita de verificación (para producción). Si no existe, usar FrontendUrl.
            string baseUrl = _configuration?["Verification:BaseUrl"]
                ?? _configuration?["FrontendUrl"]
                ?? "https://matricula.cmp.org.pe";
            string endpoint = _configuration?["Verification:Endpoint"] ?? "/verificar/documento";
            string idB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(solicitudId.ToString()));
            return string.Format("{0}{1}?id={2}", baseUrl.TrimEnd('/'), endpoint, Uri.EscapeDataString(idB64));
        }

        private void DibujarPieDePagina(PdfWriter writer, Document document, int solicitudId)
        {
            var cb = writer.DirectContent;

            float pageWidth = document.PageSize.Width;
            float left = document.LeftMargin;
            float right = document.RightMargin;
            float usableWidth = pageWidth - left - right;

            // Área reservada inferior: desde y=20 hasta y=20+_layout.BottomReservedHeight
            float baseY = 20f;
            float areaHeight = _layout.BottomReservedHeight;

            // Línea/borde inferior (más pegado al borde inferior de la página)
            cb.SaveState();
            cb.SetColorStroke(new BaseColor(128, 0, 128));
            cb.SetLineWidth(1.2f);
            float lineY = baseY + 6f;
            cb.MoveTo(left, lineY);
            cb.LineTo(left + usableWidth, lineY);
            cb.Stroke();
            cb.RestoreState();

            // Construir texto (sin enlace clicable para evitar URLs incorrectas en dev/prod)
            string url = ConstruirUrlVerificacion(solicitudId);
            string leyenda = "Esta es una copia auténtica imprimible de un documento electrónico archivado por el CMP. Su autenticidad e integridad puede verificarse en:";

            // Zona de texto (deja espacio para QR a la derecha)
            float qrMaxSize = 84f;
            float qrWidth = qrMaxSize;
            float gap = 12f;
            float textBoxWidth = usableWidth - qrWidth - gap;
            var ct = new ColumnText(cb)
            {
                Alignment = Element.ALIGN_LEFT
            };

            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var small = new Font(baseFont, 8, Font.NORMAL, BaseColor.BLACK);

            // Leyenda y URL como texto normal (sin link) para que sea correcto en cualquier entorno
            Phrase p = new Phrase();
            p.Add(new Chunk(leyenda + " ", small));
            p.Add(new Chunk(url, small));

            // Colocar el texto cerca del borde inferior, por encima de la línea
            ct.SetSimpleColumn(
                p,
                left,
                baseY + 14f,
                left + textBoxWidth,
                baseY + 40f,
                11f,
                Element.ALIGN_LEFT);
            ct.Go();

            // Generar y dibujar QR con el link
            var qr = new BarcodeQRCode(url, (int)qrMaxSize, (int)qrMaxSize, null);
            var qrImg = qr.GetImage();
            qrImg.ScaleToFit(qrMaxSize, qrMaxSize);
            float qrX = left + textBoxWidth + gap;
            float qrY = baseY + (areaHeight - qrImg.ScaledHeight) / 2f;
            qrImg.SetAbsolutePosition(qrX, qrY);
            writer.DirectContent.AddImage(qrImg);
        }
    }
}
