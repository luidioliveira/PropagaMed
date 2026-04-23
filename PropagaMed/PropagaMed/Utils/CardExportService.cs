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

        public static async Task ExportCardAsync(Medico medico)
            => await ExportCardsAsync(new List<Medico> { medico });

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

            var page = document.AddPage();
            page.Width = XUnit.FromCentimeter(21);
            page.Height = XUnit.FromCentimeter(29.7);

            using var gfx = XGraphics.FromPdfPage(page);

            double pageW = page.Width.Point;
            double pageH = page.Height.Point;
            double cardW = CardWidth.Point;
            double cardH = CardHeight.Point;
            double gap = XUnit.FromCentimeter(1).Point;

            double totalH = medicos.Count == 1 ? cardH : cardH * 2 + gap;
            double startX = (pageW - cardW) / 2;
            double startY = (pageH - totalH) / 2;

            DrawCard(gfx, medicos[0], startX, startY, cardW, cardH);

            if (medicos.Count == 2)
                DrawCard(gfx, medicos[1], startX, startY + cardH + gap, cardW, cardH);

            document.Save(outputPath);
        }

        private static bool IsNiteroi(Medico medico)
        {
            if (string.IsNullOrWhiteSpace(medico.Localizacao)) return false;

            string loc = medico.Localizacao
                .Trim()
                .ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder();
            foreach (char c in loc)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            string normalized = sb.ToString();
            return normalized == "niteroi" || normalized.StartsWith("niteroi");
        }

        private static void DrawCard(XGraphics gfx, Medico medico,
                                     double originX, double originY,
                                     double w, double h)
        {
            // ── Fontes maiores para melhor aproveitamento do cartão 11x7 cm ──────
            var fontLabel = new XFont("OpenSans", 7.0, XFontStyle.Bold);
            var fontValue = new XFont("OpenSans", 8.5, XFontStyle.Regular);
            var fontFooter = new XFont("OpenSans", 5.5, XFontStyle.Regular);

            bool niteroi = IsNiteroi(medico);

            XBrush corLabel = niteroi
                ? new XSolidBrush(XColor.FromArgb(180, 0, 0))
                : new XSolidBrush(XColor.FromArgb(80, 80, 80));

            XBrush corValor = niteroi
                ? new XSolidBrush(XColor.FromArgb(200, 0, 0))
                : XBrushes.Black;

            var corFundo = XBrushes.White;
            var corBorda = XPens.Black;
            var corLinha = new XPen(XColor.FromArgb(200, 200, 200), 0.4);

            gfx.DrawRectangle(corFundo, originX, originY, w, h);
            gfx.DrawRectangle(corBorda, originX, originY, w, h);

            // ── Layout: margens maiores, altura de linha proporcional ─────────────
            // Cartão tem ~198pt de altura. 8 campos + rodapé.
            // Reservamos: marginY*2=18, rodapé=11 → disponível = 169pt para 8 linhas
            // lineH = 169 / 8 ≈ 21pt (vs 13pt anterior, +62%)
            double marginX = 10;
            double marginY = 9;
            double rodapeH = 11;
            double lineH = (h - marginY * 2 - rodapeH) / 8.0;  // dinâmico, sempre cabe
            double colRight = originX + w * 0.52;                  // divisão levemente assimétrica
            double y = originY + marginY;

            // ── Linha 1: NOME | ESP. ──────────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "NOME", medico.Nome,
                originX + marginX, y,
                colRight - originX - marginX - 4, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "ESP.", medico.Especialidade,
                colRight, y,
                originX + w - colRight - marginX, lineH);
            y += lineH;

            // ── Linha 2: END. ─────────────────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "END.", medico.Endereco,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // ── Linha 3: CEP | ANIVERSÁRIO ────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CEP", medico.CEP,
                originX + marginX, y,
                colRight - originX - marginX - 4, lineH);

            string aniversario = medico.Aniversario == default
                ? "" : medico.Aniversario.ToString("dd/MM");
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "ANIVERSÁRIO", aniversario,
                colRight, y,
                originX + w - colRight - marginX, lineH);
            y += lineH;

            // ── Linha 4: CRM(N) ───────────────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CRM(N)", medico.CRM,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // ── Linha 5: SECRETÁRIA ───────────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "SECRETÁRIA", medico.Secretaria,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // ── Linha 6: TEL. | CELULAR ───────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "TEL.", medico.Telefone,
                originX + marginX, y,
                colRight - originX - marginX - 4, lineH);
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "CELULAR", medico.Celular,
                colRight, y,
                originX + w - colRight - marginX, lineH);
            y += lineH;

            // ── Linha 7: E-MAIL ───────────────────────────────────────────────────
            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "E-MAIL", medico.Email,
                originX + marginX, y, w - marginX * 2, lineH);
            y += lineH;

            // ── Linha 8: DIAS E TURNOS ────────────────────────────────────────────
            string diasHorarios = string.Join(" | ",
                new[] { medico.DiasVisita, medico.HorariosVisita }
                    .Where(s => !string.IsNullOrEmpty(s)));

            DrawField(gfx, fontLabel, fontValue, corLabel, corValor, corLinha,
                "DIAS E TURNOS", diasHorarios,
                originX + marginX, y, w - marginX * 2, lineH);

            // ── Rodapé ────────────────────────────────────────────────────────────
            gfx.DrawString($"PropagaMed · {DateTime.Now:dd/MM/yyyy}",
                fontFooter, XBrushes.Gray,
                new XRect(originX, originY + h - rodapeH, w, rodapeH),
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
            double valueH = height * 0.54;

            // Label
            gfx.DrawString(label, fontLabel, corLabel,
                new XRect(x, y, width, labelH),
                XStringFormats.TopLeft);

            // Valor: trunca com reticências se o texto não couber na largura disponível
            string safeValue = TruncateToFit(gfx, fontValue, value ?? "", width);
            gfx.DrawString(safeValue, fontValue, corValor,
                new XRect(x, y + labelH, width, valueH),
                XStringFormats.TopLeft);

            // Linha separadora
            gfx.DrawLine(corLinha, x, y + height - 1, x + width, y + height - 1);
        }

        /// <summary>
        /// Trunca o texto adicionando "…" caso ultrapasse a largura disponível.
        /// Garante que textos longos nunca transbordem para fora do campo.
        /// </summary>
        private static string TruncateToFit(XGraphics gfx, XFont font, string text, double maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Cabe sem corte?
            if (gfx.MeasureString(text, font).Width <= maxWidth)
                return text;

            // Reduz caracter a caracter até caber com reticências
            const string ellipsis = "…";
            double ellipsisW = gfx.MeasureString(ellipsis, font).Width;

            for (int i = text.Length - 1; i > 0; i--)
            {
                string candidate = text.Substring(0, i) + ellipsis;
                if (gfx.MeasureString(candidate, font).Width <= maxWidth)
                    return candidate;
            }

            return ellipsis;
        }

        private static string SanitizeName(string name)
            => string.IsNullOrEmpty(name)
               ? "Medico"
               : string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
    }
}