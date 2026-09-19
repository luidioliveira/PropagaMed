using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PropagaMed.Model;

namespace PropagaMed.Utils
{
    public static class MedicalReportExportService
    {
        private static bool _fontResolverSet = false;

        private static void EnsureFontResolver()
        {
            if (_fontResolverSet) return;
            GlobalFontSettings.FontResolver = PropagaMedFontResolver.Instance;
            _fontResolverSet = true;
        }

        /// <param name="cidadeFiltro">null = todas; "Rio de Janeiro" ou "Niterói" = filtrar</param>
        public static byte[] GerarRelatorio(List<Medico> medicos, string cidadeFiltro = null)
        {
            EnsureFontResolver();

            var lista = string.IsNullOrWhiteSpace(cidadeFiltro)
                ? medicos
                : medicos.Where(m =>
                    !string.IsNullOrWhiteSpace(m.Localizacao) &&
                    m.Localizacao.IndexOf(cidadeFiltro, System.StringComparison.OrdinalIgnoreCase) >= 0)
                  .ToList();

            lista = lista.OrderBy(m => m.Nome).ToList();

            if (lista.Count == 0)
                return System.Array.Empty<byte>();

            using var doc = new PdfDocument();
            var font = new XFont("OpenSans", 9, XFontStyle.Bold);
            var fontTitulo = new XFont("OpenSans", 11, XFontStyle.Bold);

            const double marginX = 40;
            const double marginY = 40;
            const double lineHeight = 13; // altura de cada linha de texto
            const double entryGap = 8;  // espaço extra entre médicos
            const double pageH = 842;
            const double pageW = 595;
            double textWidth = pageW - marginX * 2;

            PdfPage page = null;
            XGraphics gfx = null;
            double y = marginY;

            // Garante que há pelo menos uma página
            void EnsurePage()
            {
                if (page != null) return;
                page = doc.AddPage();
                page.Width = pageW;
                page.Height = pageH;
                gfx = XGraphics.FromPdfPage(page);
                y = marginY;

                gfx.DrawString("Relatório de Médicos - PropagaMed",
                    fontTitulo, XBrushes.Black,
                    new XRect(marginX, y, textWidth, lineHeight),
                    XStringFormats.TopLeft);
                y += lineHeight * 2;
            }

            EnsurePage();

            foreach (var m in lista)
            {
                var linha = FormatarLinha(m);
                var linhas = WrapText(gfx, font, linha, textWidth);

                double blocoAltura = linhas.Count * lineHeight + entryGap;

                // Quebra de página se o bloco deste médico não cabe
                if (y + blocoAltura > pageH - marginY)
                {
                    page = null;
                    EnsurePage();
                }

                foreach (var l in linhas)
                {
                    gfx.DrawString(l, font, XBrushes.Black,
                        new XRect(marginX, y, textWidth, lineHeight),
                        XStringFormats.TopLeft);
                    y += lineHeight;
                }

                y += entryGap; // espaço entre médicos
            }

            using var ms = new MemoryStream();
            doc.Save(ms, false);
            return ms.ToArray();
        }

        /// Quebra o texto em linhas que caibam dentro de maxWidth
        private static List<string> WrapText(XGraphics gfx, XFont font, string text, double maxWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            var words = text.Split(' ');
            var current = new System.Text.StringBuilder();

            foreach (var word in words)
            {
                var candidate = current.Length == 0 ? word : current + " " + word;
                var size = gfx.MeasureString(candidate, font);

                if (size.Width <= maxWidth)
                {
                    current.Clear();
                    current.Append(candidate);
                }
                else
                {
                    if (current.Length > 0)
                        result.Add(current.ToString());
                    current.Clear();
                    current.Append(word);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
        }

        private static string FormatarLinha(Medico m)
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(m.Nome))
                partes.Add(m.Nome);
            if (!string.IsNullOrWhiteSpace(m.Especialidade))
                partes.Add(m.Especialidade);

            var endereco = MontarEndereco(m);
            if (!string.IsNullOrWhiteSpace(endereco))
                partes.Add(endereco);

            if (!string.IsNullOrWhiteSpace(m.CEP))
                partes.Add($"Cep: {m.CEP}");

            if (m.Aniversario != System.DateTime.MinValue)
                partes.Add($"Aniversário {m.Aniversario:dd/MM}");

            if (!string.IsNullOrWhiteSpace(m.CRM))
                partes.Add($"CRM {m.CRM}");
            if (!string.IsNullOrWhiteSpace(m.Celular))
                partes.Add($"Celular: {m.Celular}");

            return string.Join(" / ", partes);
        }

        private static string MontarEndereco(Medico m)
        {
            var p = new List<string>();
            if (!string.IsNullOrWhiteSpace(m.Endereco)) p.Add(m.Endereco);
            return string.Join(" ", p);
        }
    }
}