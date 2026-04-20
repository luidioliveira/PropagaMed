using PropagaMed.Model;
using PropagaMed.Utils;
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
        Medico MedicoSelecionado = new();
        DateTime DataSelecionada = DateTime.Now.Date;
        private bool _modoExportacao = false;
        private readonly List<Medico> _medicosParaExportar = [];

        public Home(bool checkTodayBirthdaysHome = false, string otherView = "")
        {
            InitializeComponent();
            AlimentaMedicosEVisitas();

            this.CurrentPage = string.IsNullOrEmpty(otherView) ? this.CurrentPage : (Page)CurrentPage.FindByName(otherView);

            _ = BirthdayNotificationService.CheckTodayBirthdaysAsync(checkTodayBirthdaysHome: checkTodayBirthdaysHome);
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

            exportParameters.IsEnabled = visitas.Any();
            deleteAllVisits.IsEnabled = visitas.Any();

            medicosPicker.ItemsSource = medicos;
            listView.ItemsSource = medicos.OrderBy(m => m.Nome);
            listView2.ItemsSource = visitas.OrderBy(v => v.DiaVisita).ThenBy(v => v.HoraVisita);

            //Lógica para reforçar aniversário
            Parallel.ForEach(visitas, visita =>
            {
                var birthdayDoc = medicos.Find(m => m.Id == visita.IdMedicoVisita).Aniversario;

                if (visita.DiaVisita.Month == birthdayDoc.Month && visita.DiaVisita.Day == birthdayDoc.Day)
                {
                    visita.IsBirthday = true;
                    visita.NomeMedicoVisita += " - Aniversário!";
                }
            });
        }

        private async void CadastrarVisitaClicado(object sender, EventArgs e)
        {
            Visita VisitaASalvar = new()
            {
                IdMedicoVisita = MedicoSelecionado.Id,
                NomeMedicoVisita = MedicoSelecionado.Nome,
                DiaVisita = dataVisita.Date,
                HoraVisita = horaVisita.Time,
                TipoVisita = VisitaPresencialTipoVisita.IsChecked ? VisitaOnlineTipoVisita.IsChecked ? "Presencial e Online" : "Presencial" : VisitaOnlineTipoVisita.IsChecked ? "Online" : string.Empty,
                Observacao = obsVisita.Text is null ? "" : obsVisita.Text.ToString()
            };

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
            Medico MedicoASalvar = new();

            //Tipos, dias e horários preferenciais de visita selecionados
            string tiposVisitaSelecionados = string.Empty;
            string diasVisitaSelecionados = string.Empty;
            string horariosVisitaSelecionados = string.Empty;

            if (presencialTipoVisita.IsChecked)
                tiposVisitaSelecionados += "Presencial";
            if (onlineTipoVisita.IsChecked)
                tiposVisitaSelecionados += String.IsNullOrEmpty(tiposVisitaSelecionados) ? "Online" : "e Online";

            if (monday.IsChecked)
            {
                diasVisitaSelecionados += "Segunda";

                if (manhaMonday.IsChecked)
                    horariosVisitaSelecionados += "ManhãSegunda";
                if (tardeMonday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeSegunda" : " e TardeSegunda";
                if (noiteMonday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteSegunda" : " e NoiteSegunda";
            }

            if (tuesday.IsChecked)
            { 
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Terça" : " e Terça";

                if (manhaTuesday.IsChecked)
                    horariosVisitaSelecionados += "ManhãTerça";
                if (tardeTuesday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeTerça" : " e TardeTerça";
                if (noiteTuesday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteTerça" : " e NoiteTerça";
            }

            if (wednesday.IsChecked)
            { 
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Quarta" : " e Quarta";

                if (manhaWednesday.IsChecked)
                    horariosVisitaSelecionados += "ManhãQuarta";
                if (tardeWednesday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeQuarta" : " e TardeQuarta";
                if (noiteWednesday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteQuarta" : " e NoiteQuarta";
            }

            if (thursday.IsChecked)
            {
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Quinta" : " e Quinta";

                if (manhaThursday.IsChecked)
                    horariosVisitaSelecionados += "ManhãQuinta";
                if (tardeThursday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeQuinta" : " e TardeQuinta";
                if (noiteThursday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteQuinta" : " e NoiteQuinta";
            }

            if (friday.IsChecked)
            {
                diasVisitaSelecionados += String.IsNullOrEmpty(diasVisitaSelecionados) ? "Sexta" : " e Sexta";

                if (manhaFriday.IsChecked)
                    horariosVisitaSelecionados += "ManhãSexta";
                if (tardeFriday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeSexta" : " e TardeSexta";
                if (noiteFriday.IsChecked)
                    horariosVisitaSelecionados += String.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteSexta" : " e NoiteSexta";
            }

            MedicoASalvar.Nome = nomeMedico.Text;
            MedicoASalvar.Especialidade = espMedico.Text;
            MedicoASalvar.Localizacao = (localizacaoMedico.SelectedItem is null ? "" : localizacaoMedico.SelectedItem.ToString());
            MedicoASalvar.Endereco = enderecoMedico.Text;
            MedicoASalvar.CEP = CEPMedico.Text;
            MedicoASalvar.Aniversario = aniversarioMedico.Date;
            MedicoASalvar.Secretaria = secretaria.Text;
            MedicoASalvar.Telefone = telefoneMedico.Text;
            MedicoASalvar.Celular = celularMedico.Text;
            MedicoASalvar.Email = emailMedico.Text;
            MedicoASalvar.TiposVisita = tiposVisitaSelecionados;
            MedicoASalvar.DiasVisita = diasVisitaSelecionados;
            MedicoASalvar.HorariosVisita = horariosVisitaSelecionados;
            MedicoASalvar.CRM = CRMMedico.Text;
            MedicoASalvar.Observacao = obsMedico.Text;

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

        async void ObservacaoVisita(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            await DisplayAlert("Observação", $"{mi.CommandParameter}", "Ok");
        }

        async void DetalharVisita(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var VisitaASerDetalhada = App.Database.GetItemsVisitaAsync().Result.Where(v => v.Id == int.Parse(mi.CommandParameter.ToString())).FirstOrDefault();

            await Navigation.PushAsync(new VisitaView(VisitaASerDetalhada));
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

        void VisitasPorMedico(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);

            var visitas = App.Database.GetItemsVisitaAsync((int)mi.CommandParameter).Result.OrderBy(v => v.DiaVisita).ThenBy(v => v.HoraVisita);

            //Lógica para reforçar aniversário
            Parallel.ForEach(visitas, visita =>
            {
                var birthdayDoc = App.Database.GetItemsMedicoAsync().Result.Find(m => m.Id == visita.IdMedicoVisita).Aniversario;

                if (visita.DiaVisita.Month == birthdayDoc.Month && visita.DiaVisita.Day == birthdayDoc.Day)
                {
                    visita.IsBirthday = true;
                    visita.NomeMedicoVisita += " - Aniversário!";
                }
            });

            listView2.ItemsSource = visitas;

            allVisits.IsEnabled = true;

            this.CurrentPage = verVisitas;
        }

        void GetAllVisitsAsync(object sender, EventArgs e)
        {
            AlimentaMedicosEVisitas();
            allVisits.IsEnabled = false;

            this.CurrentPage = verVisitas;
        }

        void GetAllMedicosAsync(object sender, EventArgs e)
        {
            AlimentaMedicosEVisitas();
            allMedicos.IsEnabled = false;

            this.CurrentPage = verMedicos;
        }

        async void DeleteAllVisitsAsync(object sender, EventArgs e)
        {
            var confirmDelete = await DisplayAlert("Atenção", $"Deseja realmente deletar todas as suas visitas cadastradas?", "Sim", "Não");

            if (confirmDelete)
            { 
                await App.Database.DeleteAllVisitasAsync();
                deleteAllVisits.IsEnabled = false;
                AlimentaMedicosEVisitas();

                this.CurrentPage = verVisitas;

                await DisplayAlert("Informação", $"Visitas deletadas com sucesso", "Ok");
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

            if (_modoExportacao)
            {
                if (viewCell.BindingContext is not Medico medico) return;

                if (_medicosParaExportar.Contains(medico))
                {
                    _medicosParaExportar.Remove(medico);
                    medico.Selecionado = false;
                    viewCell.View.BackgroundColor = Color.FromHex("#FFFFFF");
                }
                else if (_medicosParaExportar.Count < 2)
                {
                    _medicosParaExportar.Add(medico);
                    medico.Selecionado = true;
                    viewCell.View.BackgroundColor = Color.FromHex("#d0f0e8");
                }
                else
                {
                    DisplayAlert("Limite atingido", "Você pode selecionar no máximo 2 médicos.", "Ok");
                }

                AtualizarBotaoExportar();
                return;
            }

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

        void OnSearchBarVisitasChanged(object sender, TextChangedEventArgs e)
        {
            var visitas = App.Database.GetItemsVisitaAsync().Result.Where(i => i.NomeMedicoVisita.ToLower().Contains(e.NewTextValue.ToLower())).OrderBy(v => v.DiaVisita).ThenBy(v => v.HoraVisita);

            //Lógica para reforçar aniversário
            Parallel.ForEach(visitas, visita =>
            {
                var birthdayDoc = App.Database.GetItemsMedicoAsync().Result.Find(m => m.Id == visita.IdMedicoVisita).Aniversario;

                if (visita.DiaVisita.Month == birthdayDoc.Month && visita.DiaVisita.Day == birthdayDoc.Day)
                {
                    visita.IsBirthday = true;
                    visita.NomeMedicoVisita += " - Aniversário!";
                }
            });

            listView2.ItemsSource = visitas;
        }

        void OnSearchBarMedicosChanged(object sender, TextChangedEventArgs e)
        {
            listView.ItemsSource = App.Database.GetItemsMedicoAsync().Result.Where(i => i.Nome.ToLower().Contains(e.NewTextValue.ToLower())).OrderBy(m => m.Nome);
        }

        private void OnButtonMedicosFilterButtonClicked(object sender, EventArgs e)
        {
            buttonMedicosFilter.IsVisible = false;
            medicosFilter.IsVisible = true;
        }

        async void OnMedicosFilterButtonClicked(object sender, EventArgs e)
        {
            var medicos = await App.Database.GetItemsMedicoAsync();
            List<Medico> filteredItems = new();

            if (RJ.IsChecked)
            {
                filteredItems.AddRange(medicos.Where(item => item.Localizacao.Contains("Rio")));
            }

            if (NIT.IsChecked)
            {
                filteredItems.AddRange(medicos.Where(item => item.Localizacao.Contains("Niterói")));
            }

            listView.ItemsSource = filteredItems.OrderBy(m => m.Nome);

            medicosFilter.IsVisible = false;
            buttonMedicosFilter.IsVisible = true;
            allMedicos.IsEnabled = true;
        }

        void OnSearchBarMedicosVisitaChanged(object sender, TextChangedEventArgs e)
        {
            medicosPicker.ItemsSource = App.Database.GetItemsMedicoAsync().Result.Where(i => i.Nome.ToLower().Contains(e.NewTextValue.ToLower())).OrderBy(m => m.Nome).ToList();
        }

        private void ModoExportacaoClicado(object sender, EventArgs e)
        {
            _modoExportacao = true;
            _medicosParaExportar.Clear();

            barraSelecao.IsVisible = true;
            btnModoExportacao.IsVisible = false;
            btnExportarCartoes.IsVisible = false;
            labelSelecao.Text = "Selecione até 2 médicos (0/2)";

            foreach (var m in listView.ItemsSource as System.Collections.IEnumerable ?? Array.Empty<object>())
                if (m is Medico medico) medico.Selecionado = false;
        }

        private void CancelarSelecaoClicado(object sender, EventArgs e)
        {
            _modoExportacao = false;
            _medicosParaExportar.Clear();

            barraSelecao.IsVisible = false;
            btnModoExportacao.IsVisible = true;
            btnExportarCartoes.IsVisible = false;
            labelSelecao.Text = "Selecione até 2 médicos";

            foreach (var m in listView.ItemsSource as System.Collections.IEnumerable ?? Array.Empty<object>())
                if (m is Medico medico) medico.Selecionado = false;
        }

        private async void ExportarCartoesClicado(object sender, EventArgs e)
        {
            if (_medicosParaExportar.Count == 0) return;

            try
            {
                btnExportarCartoes.IsEnabled = false;
                btnExportarCartoes.Text = "Gerando...";

                var medicosExport = _medicosParaExportar.Select(m => new Medico
                {
                    Id = m.Id,
                    Nome = m.Nome,
                    Especialidade = m.Especialidade,
                    Localizacao = m.Localizacao,
                    Endereco = m.Endereco,
                    CEP = m.CEP,
                    Aniversario = m.Aniversario,
                    Secretaria = m.Secretaria,
                    Telefone = m.Telefone,
                    Celular = m.Celular,
                    Email = m.Email,
                    CRM = m.CRM,
                    DiasVisita = DiasETurnosVisitaHelper.FormatarDias(m.DiasVisita),
                    HorariosVisita = DiasETurnosVisitaHelper.FormatarTurnos(m.HorariosVisita),
                    Observacao = m.Observacao
                }).ToList();

                await CardExportService.ExportCardsAsync(medicosExport);
                CancelarSelecaoClicado(sender, e);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível gerar:\n{ex.Message}", "Ok");
            }
            finally
            {
                btnExportarCartoes.IsEnabled = true;
                AtualizarBotaoExportar();
            }
        }

        private void AtualizarBotaoExportar()
        {
            int qtd = _medicosParaExportar.Count;
            btnExportarCartoes.IsVisible = qtd > 0;
            btnExportarCartoes.Text = qtd == 1 ? "Exportar Cartão" : "Exportar Cartões";
            labelSelecao.Text = $"Selecione até 2 médicos ({qtd}/2)";
        }
    }
}