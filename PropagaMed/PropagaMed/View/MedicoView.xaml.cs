using System;
using PropagaMed.Model;
using Xamarin.Forms;

namespace PropagaMed.View
{
    public partial class MedicoView : ContentPage
    {
        readonly int medicoId = default;

        public MedicoView(Medico medico)
        {
            InitializeComponent();

            medicoId = medico.Id;
            nomeMedico.Text = medico.Nome;
            espMedico.Text = medico.Especialidade;
            localizacaoMedico.SelectedItem = medico.Localizacao;
            enderecoMedico.Text = medico.Endereco;
            CEPMedico.Text = medico.CEP;
            aniversarioMedico.Date = medico.Aniversario;
            telefoneMedico.Text = medico.Telefone;
            celularMedico.Text = medico.Celular;
            emailMedico.Text = medico.Email;
            CRMMedico.Text = medico.CRM;
            //Dias preferenciais de visita
            monday.IsChecked = medico.DiasVisita.Contains("Segunda");
            tuesday.IsChecked = medico.DiasVisita.Contains("Terça");
            wednesday.IsChecked = medico.DiasVisita.Contains("Quarta");
            thursday.IsChecked = medico.DiasVisita.Contains("Quinta");
            friday.IsChecked = medico.DiasVisita.Contains("Sexta");
            //Turnos preferenciais de visita
            morning.IsChecked = medico.HorariosVisita.Contains("Manhã");
            afternoon.IsChecked = medico.HorariosVisita.Contains("Tarde");
            night.IsChecked = medico.HorariosVisita.Contains("Noite");
        }

        private async void AtualizarMedicoClicado(object sender, EventArgs e)
        {
            var returnPage = new Home(true);
            var medicoAAtualizar = new Medico
            {
                Id = medicoId
            };

            //Dias e horários preferências de visita selecionados
            string diasVisitaSelecionados = "";
            string horariosVisitaSelecionados = "";

            if (monday.IsChecked)
                diasVisitaSelecionados += "Segunda";
            if (tuesday.IsChecked)
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Terça" : " e Terça";
            if (wednesday.IsChecked)
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Quarta" : " e Quarta";
            if (thursday.IsChecked)
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Quinta" : " e Quinta";
            if (friday.IsChecked)
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Sexta" : " e Sexta";

            if (morning.IsChecked)
                horariosVisitaSelecionados += "Manhã";
            if (afternoon.IsChecked)
                horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "Tarde" : " e Tarde";
            if (night.IsChecked)
                horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "Noite" : " e Noite";

            medicoAAtualizar.Nome = nomeMedico.Text;
            medicoAAtualizar.Especialidade = espMedico.Text;
            medicoAAtualizar.Localizacao = (localizacaoMedico.SelectedItem is null ? "" : localizacaoMedico.SelectedItem.ToString());
            medicoAAtualizar.Endereco = enderecoMedico.Text;
            medicoAAtualizar.CEP = CEPMedico.Text;
            medicoAAtualizar.Aniversario = aniversarioMedico.Date;
            medicoAAtualizar.Telefone = telefoneMedico.Text;
            medicoAAtualizar.Celular = celularMedico.Text;
            medicoAAtualizar.Email = emailMedico.Text;
            medicoAAtualizar.DiasVisita = diasVisitaSelecionados;
            medicoAAtualizar.HorariosVisita = horariosVisitaSelecionados;
            medicoAAtualizar.CRM = CRMMedico.Text;

            if (!String.IsNullOrEmpty(medicoAAtualizar.Nome))
            {
                await App.Database.UpdateItemAsync(medicoAAtualizar);
                await DisplayAlert("Informação", "Cadastro do(a) Médico(a) " + medicoAAtualizar.Nome + " atualizado com sucesso", "Ok");
                returnPage.AlimentaMedicosEVisitas();
                await Navigation.PushAsync(returnPage);
            }
            else
                await DisplayAlert("Informação", "Campos necessários estão vazios", "Ok");
        }
    }
}
