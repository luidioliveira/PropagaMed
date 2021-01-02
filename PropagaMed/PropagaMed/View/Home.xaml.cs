using PropagaMed.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PropagaMed
{
    public partial class Home : TabbedPage
    {
        Medico MedicoSelecionado = new Medico();
        DateTime DataSelecionada = DateTime.Now.Date;

        public Home()
        {
            InitializeComponent();

            // Binding para data mínima ser a atual
            DateTime dateDeHoje = DateTime.Now.Date;
            dataVisita.BindingContext = dateDeHoje;

            AlimentaMedicosEVisitas();
        }

        protected async void AlimentaMedicosEVisitas()
        {
            List<Medico> medicos = await App.Database.GetItemsMedicoAsync();
            List<Visita> visitas = await App.Database.GetItemsVisitaAsync();

            medicosPicker.ItemsSource = medicos;
            listView.ItemsSource = medicos;
            listView2.ItemsSource = visitas;
        }

        private async void cadastrarVisitaClicado(object sender, EventArgs e)
        {
            Visita VisitaASalvar = new Visita();

            VisitaASalvar.idMedicoVisita = MedicoSelecionado.id;
            VisitaASalvar.nomeMedicoVisita = MedicoSelecionado.nome;
            VisitaASalvar.diaVisita = dataVisita.Date;
            VisitaASalvar.horaVisita = horaVisita.Time;
            VisitaASalvar.observacao = obsVisita.Text is null?"":obsVisita.Text.ToString();

            if (!String.IsNullOrEmpty(MedicoSelecionado.nome))
            {
                App.Database.SaveItemAsync(VisitaASalvar);
                DisplayAlert("Informação", "Visita para médico(a) " + MedicoSelecionado.nome + " às " + horaVisita.Time.ToString(@"hh\:mm") + " em " + DataSelecionada.ToString("dd/MM/yyyy") + " cadastrada com sucesso!", "Ok");
                AlimentaMedicosEVisitas();
                this.CurrentPage = verVisitas;
            }
            else
                DisplayAlert("Informação", "Campos necessários estão vazios!", "Ok");
        }

        private async void cadastrarMedicoClicado(object sender, EventArgs e)
        {
            Medico MedicoASalvar = new Medico();

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

            MedicoASalvar.nome = nomeMedico.Text;
            MedicoASalvar.especialidade = espMedico.Text;
            MedicoASalvar.localizacao = (localizacaoMedico.SelectedItem is null? "":localizacaoMedico.SelectedItem.ToString());
            MedicoASalvar.endereco = enderecoMedico.Text;
            MedicoASalvar.CEP = CEPMedico.Text;
            MedicoASalvar.aniversario = aniversarioMedico.Date;
            MedicoASalvar.telefone = telefoneMedico.Text;
            MedicoASalvar.celular = celularMedico.Text;
            MedicoASalvar.email = emailMedico.Text;
            MedicoASalvar.diasVisita = diasVisitaSelecionados;
            MedicoASalvar.horariosVisita = horariosVisitaSelecionados;
            MedicoASalvar.CRM = CRMMedico.Text;

            if (!String.IsNullOrEmpty(MedicoASalvar.nome))
            {
                App.Database.SaveItemAsync(MedicoASalvar);
                DisplayAlert("Informação", "Médico(a) " + MedicoASalvar.nome + " cadastrado(a) com sucesso!", "Ok");
                AlimentaMedicosEVisitas();
                this.CurrentPage = novaVisita;
            }
            else
                DisplayAlert("Informação", "Campos necessários estão vazios!", "Ok");
        }

        void medicosPicker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            Picker picker = sender as Picker;
            MedicoSelecionado = (Medico)picker.SelectedItem;
        }

        void dataVisita_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        {
            DatePicker picker = sender as DatePicker;
            DataSelecionada = (DateTime)picker.Date;
        }

        void DetalharVisita(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            DisplayAlert("Aguarde", $"Em desenvolvimento!", "Ok");
        }

        async void DeletarVisita(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var visitaASerDeletada = App.Database.GetItemsVisitaAsync().Result.Where(m => m.id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            await App.Database.DeleteItemAsync(visitaASerDeletada);
            AlimentaMedicosEVisitas();
            await DisplayAlert("Informação", $"Visita para {visitaASerDeletada.nomeMedicoVisita} em {visitaASerDeletada.diaVisita.ToString("dd/MM/yyyy")} deletada com sucesso!", "Ok");
        }

        void DetalharMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            DisplayAlert("Aguarde", $"Em desenvolvimento!", "Ok");
        }

        async void DeletarMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var medicoASerDeletado = App.Database.GetItemsMedicoAsync().Result.Where(m => m.id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            await App.Database.DeleteItemAsync(medicoASerDeletado);
            AlimentaMedicosEVisitas();
            await DisplayAlert("Informação", $"Médico {medicoASerDeletado.nome} deletado(a) com sucesso!", "Ok");
        }
    }
}