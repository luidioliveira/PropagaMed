using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PropagaMed.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PropagaMed.Utils
{
    public static class CardExportService
    {
        private static readonly XUnit CardWidth = XUnit.FromCentimeter(11);
        private static readonly XUnit CardHeight = XUnit.FromCentimeter(7);

        private static bool _fontResolverRegistered = false;

        // ── Entrada única (1 médico) ─────────────────────────────────────────
        public static async Task ExportCardAsync(Medico medico)
            => await ExportCardsAsync(new List<Medico> { medico });

        // ── Entrada múltipla (1 ou 2 médicos) ───────────────────────────────
        public static async Task ExportCardsAsync(List<Medico> medicos)
        {
            if (medicos == null || medicos.Count == 0) return;

            EnsureFontResolver();

            string nomes = string.Join("_", medicos.Select(m => SanitizeName(m.Nome)));
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                $"Cartoes_{nomes}_{DateTime.Now:dd_MM_yyyy}.pdf");

            GeneratePdf(medicos, filePath);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = medicos.Count == 1
                    ? $"Cartão – {medicos[0].Nome}"
                    : "Cartões de Médicos",
                File = new ShareFile(filePath, "application/pdf")
            });
        }

        private static void EnsureFontResolver()
        {
            if (_fontResolverRegistered) return;
            GlobalFontSettings.FontResolver = PropagaMedFontResolver.Instance;
            _fontResolverRegistered = true;
        }

        private static void GeneratePdf(List<Medico> medicos, string outputPath)
        {
            var document = new PdfDocument();
            document.Info.Title = medicos.Count == 1
                ? $"Cartão – {medicos[0].Nome}"
                : "PropagaMed – Cartões de Médicos";
            document.Info.Subject = "PropagaMed – Cartão de Médico";

            // Sempre A4 retrato: 21 x 29,7 cm
            var page = document.AddPage();
            page.Width = XUnit.FromCentimeter(21);
            page.Height = XUnit.FromCentimeter(29.7);

            using var gfx = XGraphics.FromPdfPage(page);

            double pageW = page.Width.Point;
            double pageH = page.Height.Point;
            double cardW = CardWidth.Point;   // 11 cm em pontos
            double cardH = CardHeight.Point;  // 7 cm em pontos
            double gap = XUnit.FromCentimeter(1).Point;

            // Altura total ocupada (1 ou 2 cartões)
            double totalH = medicos.Count == 1
                ? cardH
                : cardH * 2 + gap;

            // Centraliza horizontal e verticalmente na folha A4
            double startX = (pageW - cardW) / 2;
            double startY = (pageH - totalH) / 2;

            DrawCard(gfx, medicos[0], startX, startY, cardW, cardH);

            if (medicos.Count == 2)
                DrawCard(gfx, medicos[1], startX, startY + cardH + gap, cardW, cardH);

            document.Save(outputPath);
        }

        private static void DrawCard(XGraphics gfx, Medico medico,
                                     double originX, double originY,
                                     double w, double h)
        {
            var fontLabel = new XFont("OpenSans", 5.5, XFontStyle.Bold);
            var fontValue = new XFont("OpenSans", 6.5, XFontStyle.Regular);
            var fontFooter = new XFont("OpenSans", 4.5, XFontStyle.Regular);

            var corFundo = XBrushes.White;
            var corBorda = XPens.Black;
            var corLabel = new XSolidBrush(XColor.FromArgb(80, 80, 80));
            var corValor = XBrushes.Black;
            var corLinha = new XPen(XColor.FromArgb(200, 200, 200), 0.3);

            gfx.DrawRectangle(corFundo, originX, originY, w, h);
            gfx.DrawRectangle(corBorda, originX, originY, w, h);

            double marginX = 8;
            double marginY = 7;
            double colRight = originX + w / 2 + 4;
            double lineH = 13;
            double y = originY + marginY;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "NOME", medico.Nome,
                originX + marginX, y, colRight - originX - marginX - 6, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "ESP.", medico.Especialidade,
                colRight, y, originX + w - colRight - marginX, lineH);
            y += lineH;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "END.", medico.Endereco,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CEP", medico.CEP,
                originX + marginX, y, colRight - originX - marginX - 6, lineH);

            string aniversario = medico.Aniversario == default
                ? "" : medico.Aniversario.ToString("dd/MM");
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "ANIVERSÁRIO", aniversario,
                colRight, y, originX + w - colRight - marginX, lineH);
            y += lineH;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CRM(N)", medico.CRM,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "SECRETÁRIA", medico.Secretaria,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "TEL.", medico.Telefone,
                originX + marginX, y, colRight - originX - marginX - 6, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CELULAR", medico.Celular,
                colRight, y, originX + w - colRight - marginX, lineH);
            y += lineH;

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "E-MAIL", medico.Email,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            string diasHorarios = string.Join(" | ",
                new[] { medico.DiasVisita, medico.HorariosVisita }
                    .Where(s => !string.IsNullOrEmpty(s)));

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "DIAS E TURNOS", diasHorarios,
                originX + marginX, y, w - marginX * 2, lineH);

            gfx.DrawString($"PropagaMed · {DateTime.Now:dd/MM/yyyy}",
                fontFooter, XBrushes.Gray,
                new XRect(originX, originY + h - 8, w, 8),
                XStringFormats.BottomCenter);
        }

        private static void DrawField(
            XGraphics gfx,
            XFont fontLabel, XFont fontValue,
            XBrush corLabel, XBrush corValor, XPen corLinha,
            string label, string value,
            double x, double y, double width, double height)
        {
            double labelH = height * 0.35;
            double valueH = height * 0.55;

            gfx.DrawString(label, fontLabel, corLabel,
                new XRect(x, y, width, labelH),
                XStringFormats.TopLeft);

            gfx.DrawString(value ?? "", fontValue, corValor,
                new XRect(x, y + labelH, width, valueH),
                XStringFormats.TopLeft);

            gfx.DrawLine(corLinha, x, y + height - 1, x + width, y + height - 1);
        }

        private static string SanitizeName(string name)
            => string.IsNullOrEmpty(name)
               ? "Medico"
               : string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
    }
}