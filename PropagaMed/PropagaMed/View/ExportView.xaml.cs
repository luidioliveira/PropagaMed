using System;
using PropagaMed.Model;
using Xamarin.Forms;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PropagaMed.View
{
    public partial class ExportView : ContentPage
    {
        public ExportView()
        {
            InitializeComponent();
        }

        async void ExportToCSV(object sender, EventArgs e)
        {
            actInd.IsVisible = true;
            actInd.IsRunning = true;
            await Task.Delay(100);

            int parameter = int.Parse(((Button)sender).CommandParameter.ToString());

            string type = string.Empty;
            switch (parameter)
            {
                case (int)ExportEnum.byDay:
                    type = "para o dia";
                    break;
                case (int)ExportEnum.byMonth:
                    type = "para o mês";
                    break;
                case (int)ExportEnum.lastMonth:
                    type = "para o mês passado";
                    break;
                case (int)ExportEnum.lastSixMonths:
                    type = "para os últimos 6 meses";
                    break;
            }

            var title = $"PropagaMed - Relatório de Controle de Visitas - {DateTime.Now:dd/MM/yyyy}";
            string to = "luidi.lima@poli.ufrj.br";
            string from = "luidi.lima@poli.ufrj.br";
            string personalFolderFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).ToString() + $@"/PropagaMed_Visitas_{type}.csv";
            List<Visita> visitas = App.Database.GetItemsVisitaByParameterAsync(parameter).Result.OrderBy(v => v.HoraVisita).ToList();
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
                    content.AddRange(visitas.Select(v => string.Join(";", visitas.IndexOf(v) + 1, medicos.Where(m => m.Id.Equals(v.IdMedicoVisita)).First()?.CRM, v.NomeMedicoVisita, v.DiaVisita.ToString("dd/MM/yyyy"), v.HoraVisita.ToString(@"hh\:mm"), medicos.Where(m => m.Id.Equals(v.IdMedicoVisita)).First()?.Especialidade, v.Observacao)).ToList());
                    File.WriteAllLines(personalFolderFile, content.ToArray(), System.Text.Encoding.UTF8);
                }

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("luidi.lima@poli.ufrj.br", string.Empty/*SENHA*/);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;

                MailMessage mailSend = new MailMessage();

                mailSend.From = new MailAddress(from.Replace(';', ','));
                mailSend.To.Add(to.Replace(';', ','));
                mailSend.Subject = title;
                mailSend.SubjectEncoding = System.Text.Encoding.UTF8;
                mailSend.Body = $"Segue anexo o arquivo de visitas {type}.";
                mailSend.BodyEncoding = System.Text.Encoding.UTF8;
                mailSend.IsBodyHtml = true;
                mailSend.Attachments.Add(new Attachment(personalFolderFile, MediaTypeNames.Application.Octet));
                smtp.Send(mailSend);

                if (File.Exists(personalFolderFile))
                    File.Delete(personalFolderFile);

                await DisplayAlert("Informação", $"Arquivo enviado para o e-mail {to}", "Ok");
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
