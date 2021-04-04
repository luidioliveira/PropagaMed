using PropagaMed.Model;
using PropagaMed.View;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace PropagaMed
{
    public partial class Home : TabbedPage
    {
        Medico MedicoSelecionado = new Medico();
        DateTime DataSelecionada = DateTime.Now.Date;

        public Home(bool otherView = false)
        {
            InitializeComponent();

            // Binding para data mínima ser a atual
            DateTime dateDeHoje = DateTime.Now.Date;
            dataVisita.BindingContext = dateDeHoje;

            AlimentaMedicosEVisitas();

            this.CurrentPage =  otherView ? verMedicos : this.CurrentPage;
        }

        private void ItemTapped(object sender, System.EventArgs e)
        {
            var viewCell = (ViewCell)sender;

            if (viewCell.View.BackgroundColor.Equals(Color.FromHex("#8b8a8a")))
                viewCell.View.BackgroundColor = Color.LightBlue;
            else
                viewCell.View.BackgroundColor = Color.FromHex("#8b8a8a");
        }

        public async void AlimentaMedicosEVisitas()
        {
            List<Medico> medicos = await App.Database.GetItemsMedicoAsync();
            List<Visita> visitas = await App.Database.GetItemsVisitaAsync();

            medicosPicker.ItemsSource = medicos;
            listView.ItemsSource = medicos.OrderBy(m => m.nome);
            listView2.ItemsSource = visitas.OrderBy(v => v.diaVisita).ThenBy(v => v.horaVisita);
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
            DisplayAlert("Observação", $"{mi.CommandParameter}", "Ok");
        }

        async void DeletarVisita(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var visitaASerDeletada = App.Database.GetItemsVisitaAsync().Result.Where(m => m.id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            var confirmDelete = await DisplayAlert("Atenção", $"Deseja realmente deletar a visita para {visitaASerDeletada.nomeMedicoVisita}?", "Sim", "Não");

            if (confirmDelete)
            {
                await App.Database.DeleteItemAsync(visitaASerDeletada);
                AlimentaMedicosEVisitas();
                await DisplayAlert("Informação", $"Visita para {visitaASerDeletada.nomeMedicoVisita} em {visitaASerDeletada.diaVisita.ToString("dd/MM/yyyy")} deletada com sucesso!", "Ok");
            }
        }

        async void DetalharMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var MedicoASerConsultado = App.Database.GetItemsMedicoAsync().Result.Where(m => m.id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            await Navigation.PushAsync(new MedicoView(MedicoASerConsultado));
        }

        async void DeletarMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var medicoASerDeletado = App.Database.GetItemsMedicoAsync().Result.Where(m => m.id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            var confirmDelete = await DisplayAlert("Atenção", $"Deseja realmente deletar {medicoASerDeletado.nome}?", "Sim", "Não");

            if (confirmDelete)
            {
                await App.Database.DeleteItemAsync(medicoASerDeletado);
                AlimentaMedicosEVisitas();
                await DisplayAlert("Informação", $"Médico {medicoASerDeletado.nome} deletado(a) com sucesso!", "Ok");
            }
        }
    }
}