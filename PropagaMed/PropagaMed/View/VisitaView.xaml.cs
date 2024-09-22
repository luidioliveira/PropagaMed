using System;
using PropagaMed.Model;
using Xamarin.Forms;

namespace PropagaMed.View
{
    public partial class VisitaView : ContentPage
    {
        readonly Visita visita = new();
        DateTime DataSelecionada = DateTime.Now.Date;

        public VisitaView(Visita visita)
        {
            InitializeComponent();

            this.visita = visita;

            nomeMedico.Text = visita.NomeMedicoVisita;
            dataVisita.Date = visita.DiaVisita;
            horaVisita.Time = visita.HoraVisita;
            VisitaPresencialTipoVisita.IsChecked = visita.TipoVisita.Contains("Presencial");
            VisitaOnlineTipoVisita.IsChecked = visita.TipoVisita.Contains("Online");
            obsVisita.Text = visita.Observacao;
        }

        private async void AtualizarVisitaClicado(object sender, EventArgs e)
        {
            var returnPage = new Home("verVisitas");

            var visitaAAtualizar = new Visita
            {
                Id = visita.Id,
                IdMedicoVisita = visita.IdMedicoVisita,
                NomeMedicoVisita = visita.NomeMedicoVisita,
                DiaVisita = dataVisita.Date,
                HoraVisita = horaVisita.Time,
                TipoVisita = VisitaPresencialTipoVisita.IsChecked ? VisitaOnlineTipoVisita.IsChecked ? "Presencial e Online" : "Presencial" : VisitaOnlineTipoVisita.IsChecked ? "Online" : string.Empty,
                Observacao = obsVisita.Text is null ? "" : obsVisita.Text.ToString()
            };

            await App.Database.UpdateItemAsync(visitaAAtualizar);
            await DisplayAlert("Informação", "Cadastro da Visita do(a) médico(a) atualizado com sucesso", "Ok");
            returnPage.AlimentaMedicosEVisitas();

            await Navigation.PushAsync(returnPage);
        }

        void dataVisita_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        {
            DatePicker picker = sender as DatePicker;
            DataSelecionada = (DateTime)picker.Date;
        }
    }
}