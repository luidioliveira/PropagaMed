using System.Collections.Generic;
using PropagaMed.Model;
using PropagaMed.Utils;
using Xamarin.Forms;

namespace PropagaMed.View
{
    public partial class FiltroRelatorioPage : ContentPage
    {
        private readonly List<Medico> _medicos;

        public FiltroRelatorioPage(List<Medico> medicos)
        {
            InitializeComponent();
            _medicos = medicos;
        }

        private async void OnExportarClicked(object sender, System.EventArgs e)
        {
            try
            {
                string cidadeFiltro = null;

                if (RbRio.IsChecked)
                    cidadeFiltro = "Rio de Janeiro";
                else if (RbNiteroi.IsChecked)
                    cidadeFiltro = "Niterói";
                // RbTodas = null → sem filtro

                var pdfBytes = MedicalReportExportService.GerarRelatorio(_medicos, cidadeFiltro);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    await DisplayAlert("Aviso", "Nenhum médico encontrado com o filtro selecionado.", "OK");
                    return;
                }

                var fileName = $"{System.Guid.NewGuid().ToString("N")[..8]}_relatorio_medicos.pdf";
                var filePath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                    fileName);

                System.IO.File.WriteAllBytes(filePath, pdfBytes);

                await Xamarin.Essentials.Share.RequestAsync(new Xamarin.Essentials.ShareFileRequest
                {
                    Title = "Relatório de Médicos",
                    File = new Xamarin.Essentials.ShareFile(filePath)
                });

                await Navigation.PopAsync();
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Erro ao exportar", ex.Message, "OK");
            }
        }

        private async void OnCancelarClicked(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}