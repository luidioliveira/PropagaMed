using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PropagaMed.Model;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PropagaMed.Utils
{
    public static class CardExportService
    {
        // Proporções exatas: 11cm x 7cm
        private static readonly XUnit CardWidth  = XUnit.FromCentimeter(11);
        private static readonly XUnit CardHeight = XUnit.FromCentimeter(7);

        public static async Task ExportCardAsync(Medico medico)
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                $"Cartao_{SanitizeName(medico.Nome)}_{DateTime.Now:yyyyMMdd}.pdf");

            GeneratePdf(medico, filePath);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = $"Cartão – {medico.Nome}",
                File  = new ShareFile(filePath, "application/pdf")
            });
        }

        private static void GeneratePdf(Medico medico, string outputPath)
        {
            // Necessário para PdfSharpCore em Android/iOS sem GDI+
            if (PdfSharpCore.Utils.ImageSource.ImageSourceImpl == null)
                PdfSharpCore.Utils.ImageSource.ImageSourceImpl =
                    new PdfSharpCore.Utils.ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();

            var document = new PdfDocument();
            document.Info.Title   = $"Cartão – {medico.Nome}";
            document.Info.Subject = "PropagaMed – Cartão de Médico";

            var page = document.AddPage();
            page.Width  = CardWidth;
            page.Height = CardHeight;

            using var gfx = XGraphics.FromPdfPage(page);
            DrawCard(gfx, page, medico);

            document.Save(outputPath);
        }

        private static void DrawCard(XGraphics gfx, PdfPage page, Medico medico)
        {
            var fontLabel  = new XFont("Arial", 5.5, XFontStyle.Bold);
            var fontValue  = new XFont("Arial", 6.5, XFontStyle.Regular);
            var fontFooter = new XFont("Arial", 4.5, XFontStyle.Italic);

            var corLabel  = new XSolidBrush(XColor.FromArgb(90, 90, 90));
            var corValor  = XBrushes.Black;
            var corLinha  = new XPen(XColor.FromArgb(200, 200, 200), 0.3);

            double w = page.Width.Point;
            double h = page.Height.Point;

            // Fundo branco + borda preta
            gfx.DrawRectangle(XBrushes.White, 0, 0, w, h);
            gfx.DrawRectangle(XPens.Black, 0, 0, w, h);

            double marginX = 8;
            double marginY = 7;
            double mid     = w / 2 + 4;   // início da coluna direita
            double lineH   = 12.5;         // altura de cada campo
            double y       = marginY;

            // Linha 1 — Nome | Esp.
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "NOME", medico.Nome,
                marginX, y, mid - marginX - 6, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "ESP.", medico.Especialidade,
                mid, y, w - mid - marginX, lineH);
            y += lineH;

            // Linha 2 — Endereço
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "END.", medico.Endereco,
                marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // Linha 3 — CEP | Aniversário
            string aniversario = medico.Aniversario == default
                ? "" : medico.Aniversario.ToString("dd/MM");
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CEP", medico.CEP,
                marginX, y, mid - marginX - 6, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "ANIVERSÁRIO", aniversario,
                mid, y, w - mid - marginX, lineH);
            y += lineH;

            // Linha 4 — CRM
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CRM(N)", medico.CRM,
                marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // Linha 5 — Secretária
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "SECRETÁRIA", medico.Secretaria,
                marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // Linha 6 — Tel. | Celular
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "TEL.", medico.Telefone,
                marginX, y, mid - marginX - 6, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CELULAR", medico.Celular,
                mid, y, w - mid - marginX, lineH);
            y += lineH;

            // Linha 7 — E-mail
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "E-MAIL", medico.Email,
                marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // Linha 8 — Dias e Horários
            string diasHorarios = string.Join(" | ",
                new[] { medico.DiasVisita, medico.HorariosVisita }
                    .Where(s => !string.IsNullOrEmpty(s)));
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "DIAS E HORÁRIOS", diasHorarios,
                marginX, y, w - marginX * 2, lineH);

            // Rodapé
            gfx.DrawString($"PropagaMed · {DateTime.Now:dd/MM/yyyy}",
                fontFooter, XBrushes.Gray,
                new XRect(0, h - 8, w, 8),
                XStringFormats.BottomCenter);
        }

        private static void DrawField(
            XGraphics gfx,
            XFont fontLabel, XFont fontValue,
            XBrush corLabel, XBrush corValor, XPen corLinha,
            string label, string value,
            double x, double y, double width, double height)
        {
            double labelH = height * 0.36;
            double valueH = height * 0.55;

            gfx.DrawString(label, fontLabel, corLabel,
                new XRect(x, y, width, labelH),
                XStringFormats.TopLeft);

            gfx.DrawString(value ?? "", fontValue, corValor,
                new XRect(x, y + labelH, width, valueH),
                XStringFormats.TopLeft);

            double lineY = y + height - 1;
            gfx.DrawLine(corLinha, x, lineY, x + width, lineY);
        }

        private static string SanitizeName(string name)
            => string.IsNullOrEmpty(name)
               ? "Medico"
               : string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
    }
}
