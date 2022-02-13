using PropagaMed.Model;
using PropagaMed.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
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

            exportToCSV.Text = $"Exportar Visitas do Dia {DateTime.Now:dd/MM/yyyy}";

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

            var confirmDelete = await DisplayAlert("Atenção", $"Deseja realmente deletar {medicoASerDeletado.Nome}?", "Sim", "Não");

            if (confirmDelete)
            {
                await App.Database.DeleteItemAsync(medicoASerDeletado);
                AlimentaMedicosEVisitas();
                await DisplayAlert("Informação", $"Médico {medicoASerDeletado.Nome} deletado(a) com sucesso", "Ok");
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

        async void ExportToCSV(object sender, EventArgs e)
        {
            actInd.IsVisible = true;
            actInd.IsRunning = true;
            await Task.Delay(100);

            var title = $"PropagaMed - Relatório de Controle de Visitas - {DateTime.Now:dd/MM/yyyy}";
            string to = "luidi.lima@poli.ufrj.br";
            string from = "luidi.lima@poli.ufrj.br";
            string personalFolderFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).ToString() + $@"/PropagaMed_Visitas_{DateTime.Now:dd-MM-yyyy}.csv";
            List<Visita> visitas = App.Database.GetItemsVisitaDiaAsync().Result.OrderBy(v => v.HoraVisita).ToList();
            List<Medico> medicos = new List<Medico>();

            if (visitas.Any())
            {
                Parallel.ForEach(visitas, visita =>
                {
                    medicos.Add(App.Database.GetItemAsync(visita.IdMedicoVisita).Result);
                });

                if (!File.Exists(personalFolderFile))
                {
                    var content = new List<string>() { "Controle de Visitas", "Nº;CRM;Nome;Data;Hora;Especialidade;Observação;" };
                    var toAdd = visitas.Select(v => string.Join(";", visitas.IndexOf(v) + 1, medicos.Where(m => m.Id.Equals(v.IdMedicoVisita)).First()?.CRM, v.NomeMedicoVisita, v.DiaVisita.ToString("dd/MM/yyyy"), v.HoraVisita.ToString(@"hh\:mm"), medicos.Where(m => m.Id.Equals(v.IdMedicoVisita)).First()?.Especialidade, v.Observacao)).ToList();

                    content.AddRange(toAdd);

                    File.WriteAllLines(personalFolderFile, content.ToArray(), System.Text.Encoding.UTF8);
                }

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("luidi.lima@poli.ufrj.br", string.Empty/*SENHA AQUI*/);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;

                MailMessage mailSend = new MailMessage();

                mailSend.From = new MailAddress(from.Replace(';', ','));
                mailSend.To.Add(to.Replace(';', ','));
                mailSend.Subject = title;
                mailSend.SubjectEncoding = System.Text.Encoding.UTF8;
                mailSend.Body = "Anexo.";
                mailSend.BodyEncoding = System.Text.Encoding.UTF8;
                mailSend.IsBodyHtml = true;
                mailSend.Attachments.Add(new Attachment(personalFolderFile, MediaTypeNames.Application.Octet));
                smtp.Send(mailSend);

                if (File.Exists(personalFolderFile))
                    File.Delete(personalFolderFile);

                await DisplayAlert("Informação", $"Arquivo enviado por e-mail", "Ok");
            }
            else
            {
                await DisplayAlert("Informação", $"Não há visitas para {DateTime.Now:dd/MM/yyyy}", "Ok");
            }

            actInd.IsVisible = false;
            actInd.IsRunning = false;
        }
    }
}