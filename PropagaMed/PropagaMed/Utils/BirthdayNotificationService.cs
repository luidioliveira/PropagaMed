using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PropagaMed.Utils
{
    public static class BirthdayNotificationService
    {
        public static async Task CheckTodayBirthdaysAsync(bool checkTodayBirthdaysHome = false)
        {
            try
            {
                var currentPage = (Application.Current.MainPage as NavigationPage)?.CurrentPage;
                if (currentPage is Login && !checkTodayBirthdaysHome)
                    return;

                var today = DateTime.Now.Date;

                var visitasHoje = (await App.Database.GetItemsVisitaAsync())
                    .Where(v => v.DiaVisita.Date == today)
                    .ToList();

                if (!visitasHoje.Any()) return;

                var medicos = await App.Database.GetItemsMedicoAsync();

                var aniversariantes = visitasHoje
                    .Select(v => new
                    {
                        Visita = v,
                        Medico = medicos.FirstOrDefault(m => m.Id == v.IdMedicoVisita)
                    })
                    .Where(x => x.Medico != null
                             && x.Visita.DiaVisita.Month == x.Medico.Aniversario.Month
                             && x.Visita.DiaVisita.Day == x.Medico.Aniversario.Day)
                    .ToList();

                if (!aniversariantes.Any()) return;

                var sb = new StringBuilder();
                sb.AppendLine("Você tem visitas hoje com médicos(as) que fazem aniversário\n");

                foreach (var item in aniversariantes)
                {
                    sb.AppendLine($"> Dr(a). {item.Medico.Nome}");
                    sb.AppendLine($"> {item.Visita.HoraVisita:hh\\:mm} – {item.Visita.TipoVisita}");
                    if (!string.IsNullOrEmpty(item.Medico.Especialidade))
                        sb.AppendLine($"> Esp.: {item.Medico.Especialidade}");
                    sb.AppendLine();
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        aniversariantes.Count == 1
                            ? $"Aniversário hoje! {DateTime.Now.Date:dd/MM/yyyy}"
                            : $"{aniversariantes.Count} aniversários hoje! {DateTime.Now.Date:dd/MM/yyyy}",
                        sb.ToString(),
                        "Ok");
                });
            }
            catch { }
        }
    }
}