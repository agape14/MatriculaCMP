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
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Configurar fuentes
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                BaseFont gothicFont = BaseFont.CreateFont("c:/windows/fonts/GOTHIC.TTF", BaseFont.CP1252, BaseFont.EMBEDDED);
                Font titleFont = new Font(gothicFont, 28, Font.BOLD);
                Font subtitleFont = new Font(gothicFont, 18, Font.BOLD);
                Font normalFont = new Font(baseFont, 12, Font.NORMAL);
                Font smallFont = new Font(baseFont, 10, Font.NORMAL);

                // Agregar logo (centrado)
                try
                {
                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "assets", "images", "logo-cmp.png");
                    if (File.Exists(logoPath))
                    {
                        Image logo = Image.GetInstance(logoPath);
                        float maxWidth = document.PageSize.Width * 0.3f; // 30% del ancho de la página
                        float maxHeight = document.PageSize.Height * 0.15f; // 15% del alto de la página
                        logo.ScaleToFit(maxWidth, maxHeight);
                        logo.Alignment = Element.ALIGN_CENTER;
                        logo.SpacingAfter = 20f;
                        document.Add(logo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar el logo: {ex.Message}");
                    // Continuar sin el logo
                }

                // Título principal
                Paragraph header = new Paragraph();
                //header.Add(new Chunk("El\n", titleFont));
                header.Add(new Chunk("El Colegio Médico del Perú", titleFont));
                header.Alignment = Element.ALIGN_CENTER;
                header.SpacingAfter = 40f;
                document.Add(header);

                // Por cuanto
                Paragraph porCuanto = new Paragraph("Por cuanto, el médico cirujano", subtitleFont);
                porCuanto.Alignment = Element.ALIGN_CENTER;
                porCuanto.SpacingAfter = 30f;
                document.Add(porCuanto);

                // Nombre completo
                Paragraph nombre = new Paragraph(diploma.NombreCompleto.ToUpper(), titleFont);
                nombre.Alignment = Element.ALIGN_CENTER;
                nombre.SpacingAfter = 30f;
                document.Add(nombre);

                // Graduado en
                Paragraph graduado = new Paragraph("Graduado en:", subtitleFont);
                graduado.Alignment = Element.ALIGN_CENTER;
                graduado.SpacingAfter = 20f;
                document.Add(graduado);

                // Universidad seleccionada
                var universidadNombre = string.IsNullOrWhiteSpace(diploma.UniversidadNombre) ? "" : diploma.UniversidadNombre.ToUpper();
                Paragraph especialidad = new Paragraph(universidadNombre, titleFont);
                especialidad.Alignment = Element.ALIGN_CENTER;
                especialidad.SpacingAfter = 30f;
                document.Add(especialidad);

                // Texto principal
                Paragraph mainText = new Paragraph(
                    $"ha cumplido con las disposiciones estatutarias y reglamentarias vigentes y está inscrito en el Registro Nacional de Matrículas, con el N° {diploma.NumeroColegiatura}.", 
                    normalFont);
                mainText.Alignment = Element.ALIGN_JUSTIFIED;
                mainText.SpacingAfter = 30f;
                mainText.IndentationLeft = 50f;
                mainText.IndentationRight = 50f;
                document.Add(mainText);

                // Por tanto
                Paragraph porTanto = new Paragraph("Por tanto,", subtitleFont);
                porTanto.Alignment = Element.ALIGN_CENTER;
                porTanto.SpacingAfter = 20f;
                document.Add(porTanto);

                // Texto final
                Paragraph finalText = new Paragraph(
                    "se le expide el presente Certificado, que lo acredita como colegiado y lo faculta para el ejercicio de la profesión en el territorio de la República.", 
                    normalFont);
                finalText.Alignment = Element.ALIGN_JUSTIFIED;
                finalText.SpacingAfter = 50f;
                finalText.IndentationLeft = 50f;
                finalText.IndentationRight = 50f;
                document.Add(finalText);

                // Fecha y lugar
                Paragraph fechaLugar = new Paragraph($"Lima, {diploma.FechaEmision:dd} de {GetMonthName(diploma.FechaEmision.Month)} del {diploma.FechaEmision.Year}", normalFont);
                fechaLugar.Alignment = Element.ALIGN_CENTER;
                fechaLugar.SpacingBefore = 50f;
                document.Add(fechaLugar);

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
