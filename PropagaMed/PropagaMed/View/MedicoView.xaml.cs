using System;
using PropagaMed.Model;
using Xamarin.Forms;

namespace PropagaMed.View
{
    public partial class MedicoView : ContentPage
    {
        int medicoId = default;

        public MedicoView(Medico medico)
        {
            InitializeComponent();

            medicoId = medico.id;
            nomeMedico.Text = medico.nome;
            espMedico.Text = medico.especialidade;
            localizacaoMedico.SelectedItem = medico.localizacao;
            enderecoMedico.Text = medico.endereco;
            CEPMedico.Text = medico.CEP;
            aniversarioMedico.Date = medico.aniversario;
            telefoneMedico.Text = medico.telefone;
            celularMedico.Text = medico.celular;
            emailMedico.Text = medico.email;
            CRMMedico.Text = medico.CRM;
            //Dias preferenciais de visita
            monday.IsChecked = medico.diasVisita.Contains("Segunda");
            tuesday.IsChecked = medico.diasVisita.Contains("Terça");
            wednesday.IsChecked = medico.diasVisita.Contains("Quarta");
            thursday.IsChecked = medico.diasVisita.Contains("Quinta");
            friday.IsChecked = medico.diasVisita.Contains("Sexta");
            //Turnos preferenciais de visita
            morning.IsChecked = medico.horariosVisita.Contains("Manhã");
            afternoon.IsChecked = medico.horariosVisita.Contains("Tarde");
            night.IsChecked = medico.horariosVisita.Contains("Noite");
        }

        private async void atualizarMedicoClicado(object sender, EventArgs e)
        {
            var returnPage = new Home(true);
            var medicoAAtualizar = new Medico();

            medicoAAtualizar.id = medicoId;

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

            medicoAAtualizar.nome = nomeMedico.Text;
            medicoAAtualizar.especialidade = espMedico.Text;
            medicoAAtualizar.localizacao = (localizacaoMedico.SelectedItem is null ? "" : localizacaoMedico.SelectedItem.ToString());
            medicoAAtualizar.endereco = enderecoMedico.Text;
            medicoAAtualizar.CEP = CEPMedico.Text;
            medicoAAtualizar.aniversario = aniversarioMedico.Date;
            medicoAAtualizar.telefone = telefoneMedico.Text;
            medicoAAtualizar.celular = celularMedico.Text;
            medicoAAtualizar.email = emailMedico.Text;
            medicoAAtualizar.diasVisita = diasVisitaSelecionados;
            medicoAAtualizar.horariosVisita = horariosVisitaSelecionados;
            medicoAAtualizar.CRM = CRMMedico.Text;

            if (!String.IsNullOrEmpty(medicoAAtualizar.nome))
            {
                await App.Database.UpdateItemAsync(medicoAAtualizar);
                await DisplayAlert("Informação", "Cadastro do(a) Médico(a) " + medicoAAtualizar.nome + " atualizado com sucesso!", "Ok");
                returnPage.AlimentaMedicosEVisitas();
                await Navigation.PushAsync(returnPage);
            }
            else
                await DisplayAlert("Informação", "Campos necessários estão vazios!", "Ok");

            //await DisplayAlert("Aguarde", $"Em desenvolvimento!", "Ok");
        }
    }
}
