using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using MatriculaCMP.Shared;
using System.IO;

namespace MatriculaCMP.Services
{
    public class PdfService
    {
        public byte[] GenerarDiplomaPdf(Diploma diploma)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Reducir aún más el margen superior para acercar el contenido al borde superior
                Document document = new Document(PageSize.A4, 50, 50, 10, 40);
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
                Font fixedFont26 = new Font(oldEnglishBase, 26, Font.NORMAL);
                Font fixedFont18 = new Font(oldEnglishBase, 18, Font.NORMAL);
                // Dinámico: Helvetica (o fuente estándar) para que sea más legible en una sola línea
                Font dynamicFont26 = new Font(baseFont, 26, Font.BOLD);
                Font dynamicFont22 = new Font(baseFont, 22, Font.BOLD);
                Font dynamicFont18 = new Font(baseFont, 18, Font.BOLD);

                // Agregar logo (centrado)
                try
                {
                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "assets", "images", "logotipo-cmp.png");
                    if (File.Exists(logoPath))
                    {
                        Image logo = Image.GetInstance(logoPath);
                        float maxWidth = document.PageSize.Width * 0.3f; // 30% del ancho de la página
                        float maxHeight = document.PageSize.Height * 0.15f; // 15% del alto de la página
                        logo.ScaleToFit(maxWidth, maxHeight);
                        logo.Alignment = Element.ALIGN_CENTER;
                        logo.SpacingAfter = 0.15f;
                        document.Add(logo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar el logo: {ex.Message}");
                    // Continuar sin el logo
                }

                // Título principal (en dos líneas: "El" y debajo el título)
                Paragraph elTitulo = new Paragraph(new Chunk("El", titleSmallFont))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 0f
                };
                document.Add(elTitulo);

                Paragraph titulo = new Paragraph(new Chunk("Colegio Médico del Perú", titleBigFont))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 0f
                };
                document.Add(titulo);

                // Por cuanto
                Paragraph porCuanto = new Paragraph("Por cuanto, el médico cirujano", fixedFont26);
                porCuanto.Alignment = Element.ALIGN_LEFT;
                porCuanto.IndentationLeft = 40f;
                porCuanto.SpacingAfter = 4f;
                document.Add(porCuanto);

                // Nombre completo (dinámico)
                var nombreTexto = (diploma.NombreCompleto ?? string.Empty).ToUpper();
                float contenidoAncho = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                float tamNombre = 26f;
                while (baseFont.GetWidthPoint(nombreTexto, tamNombre) > contenidoAncho && tamNombre > 18f)
                {
                    tamNombre -= 0.5f;
                }
                var nombreFont = new Font(baseFont, tamNombre, Font.BOLD);
                Paragraph nombre = new Paragraph(nombreTexto, nombreFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 2f
                };
                document.Add(nombre);
                // Línea bajo el nombre
                var nombreLinea = new LineSeparator(0.8f, 70f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f);
                var nombreLineaPar = new Paragraph();
                nombreLineaPar.Add(new Chunk(nombreLinea));
                nombreLineaPar.SpacingAfter = 4f;
                document.Add(nombreLineaPar);

                // Graduado en
                Paragraph graduado = new Paragraph("Graduado en:", fixedFont26);
                graduado.Alignment = Element.ALIGN_LEFT;
                graduado.IndentationLeft = 40f;
                graduado.SpacingAfter = 4f;
                document.Add(graduado);

                // Universidad seleccionada
                var universidadNombre = string.IsNullOrWhiteSpace(diploma.UniversidadNombre) ? string.Empty : diploma.UniversidadNombre.ToUpper();
                float tamUni = 26f;
                while (baseFont.GetWidthPoint(universidadNombre, tamUni) > contenidoAncho && tamUni > 18f)
                {
                    tamUni -= 0.5f;
                }
                var uniFont = new Font(baseFont, tamUni, Font.BOLD);
                Paragraph especialidad = new Paragraph(universidadNombre, uniFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 2f
                };
                document.Add(especialidad);
                // Línea bajo universidad
                var uniLinea = new LineSeparator(0.8f, 80f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f);
                var uniLineaPar = new Paragraph();
                uniLineaPar.Add(new Chunk(uniLinea));
                uniLineaPar.SpacingAfter = 4f;
                document.Add(uniLineaPar);

                // Texto principal en 26, con número de colegiatura en fuente dinámica
                Paragraph mainText = new Paragraph ("ha cumplidores con las disposiciones estatutarias y reglamentarias vigentes y está inscrito en el Registro Nacional de Matrículas, con el N° ",
                fixedFont26);
                
                // Seguir exactamente el formato/separación de finalText
                mainText.Alignment = Element.ALIGN_JUSTIFIED;
                mainText.Add(new Chunk(diploma.NumeroColegiatura ?? string.Empty, dynamicFont18));
                mainText.Add(new Chunk(".", fixedFont26));
                mainText.SetLeading(0, 1.2f);
                mainText.SpacingAfter = 4f;
                mainText.IndentationLeft = 40f; 
                mainText.IndentationRight = 40f;
                document.Add(mainText);

                // Por tanto
                Paragraph porTanto = new Paragraph("Por tanto,", fixedFont26);
                porTanto.Alignment = Element.ALIGN_LEFT;
                porTanto.IndentationLeft = 40f;
                porTanto.SpacingAfter = 4f;
                document.Add(porTanto);

                // Texto final
                Paragraph finalText = new Paragraph(
                    "se le expide el presente Certificado, que lo acredita como colegiado y lo faculta para el ejercicio de la profesión en el territorio de la República.",
                    fixedFont26);
                finalText.Alignment = Element.ALIGN_JUSTIFIED;
                // Interlineado moderado
                finalText.SetLeading(0, 1.2f);
                finalText.SpacingAfter = 4f;
                finalText.IndentationLeft = 40f;
                finalText.IndentationRight = 40f;
                document.Add(finalText);

                // Fecha y lugar (18)
                var fechaTexto = new Paragraph { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 18f };
                fechaTexto.IndentationLeft = 40f;
                fechaTexto.IndentationRight = 40f;
                fechaTexto.Add(new Chunk("Lima, ", fixedFont18));
                fechaTexto.Add(new Chunk(diploma.FechaEmision.ToString("dd"), fixedFont18));
                fechaTexto.Add(new Chunk(" de ", fixedFont18));
                fechaTexto.Add(new Chunk(GetMonthName(diploma.FechaEmision.Month), fixedFont18));
                fechaTexto.Add(new Chunk(" del ", fixedFont18));
                fechaTexto.Add(new Chunk(diploma.FechaEmision.Year.ToString(), fixedFont18));
                document.Add(fechaTexto);

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
    }
}
