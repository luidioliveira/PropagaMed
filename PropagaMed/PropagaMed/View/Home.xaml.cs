using PropagaMed.Model;
using PropagaMed.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            AlimentaMedicosEVisitas();

            this.CurrentPage = otherView ? verMedicos : this.CurrentPage;
        }

        private void ItemTapped(object sender, System.EventArgs e)
        {
            var viewCell = (ViewCell)sender;

            if (viewCell.View.BackgroundColor.Equals(Color.FromHex("#FFFFFF")))
                viewCell.View.BackgroundColor = Color.LightBlue;
            else
                viewCell.View.BackgroundColor = Color.FromHex("#FFFFFF");
        }

        public async void AlimentaMedicosEVisitas()
        {
            List<Medico> medicos = await App.Database.GetItemsMedicoAsync();
            List<Visita> visitas = await App.Database.GetItemsVisitaAsync();

            medicosPicker.ItemsSource = medicos;
            listView.ItemsSource = medicos.OrderBy(m => m.Nome);
            listView2.ItemsSource = visitas.OrderByDescending(v => v.DiaVisita).ThenBy(v => v.HoraVisita);
        }

        private async void CadastrarVisitaClicado(object sender, EventArgs e)
        {
            Visita VisitaASalvar = new Visita();

            VisitaASalvar.IdMedicoVisita = MedicoSelecionado.Id;
            VisitaASalvar.NomeMedicoVisita = MedicoSelecionado.Nome;
            VisitaASalvar.DiaVisita = dataVisita.Date;
            VisitaASalvar.HoraVisita = horaVisita.Time;
            VisitaASalvar.Observacao = obsVisita.Text is null ? "" : obsVisita.Text.ToString();

            if (!String.IsNullOrEmpty(MedicoSelecionado.Nome))
            {
                await App.Database.SaveItemAsync(VisitaASalvar);
                AlimentaMedicosEVisitas();
                this.CurrentPage = verVisitas;
            }
            else
                await DisplayAlert("Informação", "Campos necessários estão vazios", "Ok");
        }

        private async void CadastrarMedicoClicado(object sender, EventArgs e)
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

            MedicoASalvar.Nome = nomeMedico.Text;
            MedicoASalvar.Especialidade = espMedico.Text;
            MedicoASalvar.Localizacao = (localizacaoMedico.SelectedItem is null ? "" : localizacaoMedico.SelectedItem.ToString());
            MedicoASalvar.Endereco = enderecoMedico.Text;
            MedicoASalvar.CEP = CEPMedico.Text;
            MedicoASalvar.Aniversario = aniversarioMedico.Date;
            MedicoASalvar.Telefone = telefoneMedico.Text;
            MedicoASalvar.Celular = celularMedico.Text;
            MedicoASalvar.Email = emailMedico.Text;
            MedicoASalvar.DiasVisita = diasVisitaSelecionados;
            MedicoASalvar.HorariosVisita = horariosVisitaSelecionados;
            MedicoASalvar.CRM = CRMMedico.Text;

            if (!String.IsNullOrEmpty(MedicoASalvar.Nome))
            {
                await App.Database.SaveItemAsync(MedicoASalvar);
                AlimentaMedicosEVisitas();
                this.CurrentPage = novaVisita;
            }
            else
                await DisplayAlert("Informação", "Campos necessários estão vazios", "Ok");
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
            var visitaASerDeletada = App.Database.GetItemsVisitaAsync().Result.Where(m => m.Id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            var confirmDelete = await DisplayAlert("Atenção", $"Deseja realmente deletar a visita para {visitaASerDeletada.NomeMedicoVisita}?", "Sim", "Não");

            if (confirmDelete)
            {
                await App.Database.DeleteItemAsync(visitaASerDeletada);
                AlimentaMedicosEVisitas();
                await DisplayAlert("Informação", $"Visita para {visitaASerDeletada.NomeMedicoVisita} em {visitaASerDeletada.DiaVisita.ToString("dd/MM/yyyy")} deletada com sucesso", "Ok");
            }
        }

        async void DetalharMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var MedicoASerConsultado = App.Database.GetItemsMedicoAsync().Result.Where(m => m.Id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            await Navigation.PushAsync(new MedicoView(MedicoASerConsultado));
        }

        async void DeletarMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var medicoASerDeletado = App.Database.GetItemsMedicoAsync().Result.Where(m => m.Id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            var confirmDelete = await DisplayAlert("Atenção", $"Deseja realmente deletar {medicoASerDeletado.Nome}? Essa ação não poderá ser desfeita e também removerá as visitas associadas. Deseja prosseguir?", "Sim", "Não");

            if (confirmDelete)
            {
                await App.Database.DeleteItemAsync(medicoASerDeletado);

                Parallel.ForEach(App.Database.GetItemsVisitaAsync().Result.Where(v => v.IdMedicoVisita.Equals(medicoASerDeletado.Id)), visitaASerDeletada =>
                {
                    App.Database.DeleteItemAsync(visitaASerDeletada);
                });

                AlimentaMedicosEVisitas();
                await DisplayAlert("Informação", $"Médico(a) {medicoASerDeletado.Nome} e visitas associadas deletados com sucesso", "Ok");
            }
        }

        void ViewSelected(object sender, EventArgs e)
        {
            var viewCell = (ViewCell)sender;
            var actualColor = viewCell.View.BackgroundColor.ToHex();

            if (actualColor.Equals("#FFFFFF"))
                viewCell.View.BackgroundColor = Color.FromHex("#98bee3");
            else
                viewCell.View.BackgroundColor = Color.FromHex("#FFFFFF");
        }

        async void ExportParameters(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ExportView());
        }
    }
}