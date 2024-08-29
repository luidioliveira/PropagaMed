using System;
using PropagaMed.Model;
using Xamarin.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PropagaMed.View
{
    public partial class ExportView : ContentPage
    {
        public ExportView()
        {
            InitializeComponent();
        }

        [Obsolete]
        async void ExportToCSV(object sender, EventArgs e)
        {
            actInd.IsVisible = true;
            actInd.IsRunning = true;
            await Task.Delay(100);

            int parameter = int.Parse(((Button)sender).CommandParameter.ToString());

            string type = string.Empty;
            string typeAdd = string.Empty;

            switch (parameter)
            {
                case (int)ExportEnum.byDay:
                    type = "para o dia";
                    typeAdd = $"{DateTime.Now:dd-MM-yyyy}";
                    break;
                case (int)ExportEnum.byMonth:
                    type = "para o mês";
                    typeAdd = $"Mes_{DateTime.Now.Month}";
                    break;
                case (int)ExportEnum.lastMonth:
                    type = "para o mês passado";
                    typeAdd = $"Mes_{DateTime.Now.AddMonths(-1).Month}";
                    break;
                case (int)ExportEnum.lastSixMonths:
                    type = "para os últimos 6 meses";
                    typeAdd = "Ultimos_6_meses";
                    break;
            }

            string personalFolderFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).ToString() + $@"/PropagaMed_Visitas_{typeAdd}.csv";
            List<Visita> visitas = App.Database.GetItemsVisitaByParameterAsync(parameter).Result.OrderBy(v => v.HoraVisita).ToList();
            List<Medico> medicos = new();

            if (visitas.Any())
            {
                Parallel.ForEach(visitas, visita =>
                {
                    medicos.Add(App.Database.GetItemAsync(visita.IdMedicoVisita).Result);
                });

                if (!File.Exists(personalFolderFile))
                {
                    List<string> content = new() { "Controle de Visitas", "Nº;CRM;Nome;Data;Hora;Especialidade;Observação;" };
                    content.AddRange(visitas.Select(v => string.Join(";", visitas.IndexOf(v) + 1, medicos.Where(m => m.Id.Equals(v.IdMedicoVisita)).First()?.CRM, v.NomeMedicoVisita, v.DiaVisita.ToString("dd/MM/yyyy"), v.HoraVisita.ToString(@"hh\:mm"), medicos.Where(m => m.Id.Equals(v.IdMedicoVisita)).First()?.Especialidade, v.Observacao)).ToList());
                    File.WriteAllLines(personalFolderFile, [.. content], System.Text.Encoding.UTF8);
                }

                //Envio de e-mail
                var client = new SendGridClient(""); //API Key
                var from = new EmailAddress("luidi.lima@poli.ufrj.br", "PropagaMed");
                var subject = $"PropagaMed - Relatório de Controle de Visitas - {DateTime.Now:dd/MM/yyyy}";
                var to = new EmailAddress("luidi.lima@poli.ufrj.br", "Luidi Oliveira");
                var plainTextContent = $"Segue anexo o arquivo de visitas {type}.";
                var htmlContent = $"<strong>Segue anexo o arquivo de visitas {type}.</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                //Relatório como anexo
                var fileBytes = File.ReadAllBytes(personalFolderFile);
                var fileBase64 = Convert.ToBase64String(fileBytes);
                msg.AddAttachment($"PropagaMed_Visitas_{typeAdd}.csv", fileBase64);
                var response = await client.SendEmailAsync(msg);

                if (File.Exists(personalFolderFile))
                    File.Delete(personalFolderFile);

                if (response.IsSuccessStatusCode)
                { 
                    await DisplayAlert("Informação", $"Arquivo enviado para o e-mail {to.Email}", "Ok");
                }
                else
                    await DisplayAlert("Erro", $"Falha no envio para o e-mail {to.Email}. Tente novamente mais tarde.", "Ok");
            }
            else
            {
                await DisplayAlert("Informação", $"Não há visitas para exportação {type}", "Ok");
            }

            actInd.IsVisible = false;
            actInd.IsRunning = false;
        }
    }
}
