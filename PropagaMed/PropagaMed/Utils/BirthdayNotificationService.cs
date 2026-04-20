using PropagaMed.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PropagaMed.Utils
{
    public static class BirthdayNotificationService
    {
        /// <summary>
        /// Verifica se a visita ocorre no aniversário do médico.
        /// Exibe alerta e permite registrar observação comemorativa.
        /// </summary>
        public static bool CheckAndNotify(Visita visita, Medico medico)
        {
            if (medico == null || medico.Aniversario == default) return false;

            bool isBirthday = visita.DiaVisita.Month == medico.Aniversario.Month
                           && visita.DiaVisita.Day   == medico.Aniversario.Day;

            if (isBirthday)
            {
                visita.IsBirthday = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool addObs = await Application.Current.MainPage.DisplayAlert(
                        "🎂 Aniversário!",
                        $"A visita de {visita.DiaVisita:dd/MM/yyyy} coincide com o aniversário de {medico.Nome}!\n" +
                        "Ótima oportunidade para levar um brinde ou cartão.",
                        "Registrar observação",
                        "Ok");

                    if (addObs)
                    {
                        string obs = await Application.Current.MainPage.DisplayPromptAsync(
                            "Observação de Aniversário",
                            $"Nota para a visita de aniversário de {medico.Nome}:",
                            initialValue: "🎂 Visita de aniversário – levar brinde");

                        if (!string.IsNullOrWhiteSpace(obs))
                        {
                            visita.Observacao = obs;
                            await App.Database.SaveItemAsync(visita);
                        }
                    }
                });
            }

            return isBirthday;
        }

        /// <summary>
        /// Decora a lista de visitas com a flag de aniversário de forma assíncrona e segura.
        /// Substitui os blocos Parallel.ForEach duplicados em Home.xaml.cs.
        /// </summary>
        public static Task DecorateBirthdaysAsync(IEnumerable<Visita> visitas, List<Medico> medicos)
        {
            foreach (var visita in visitas)
            {
                var medico = medicos.FirstOrDefault(m => m.Id == visita.IdMedicoVisita);
                if (medico == null || medico.Aniversario == default) continue;

                bool isBirthday = visita.DiaVisita.Month == medico.Aniversario.Month
                               && visita.DiaVisita.Day   == medico.Aniversario.Day;

                if (isBirthday && !visita.NomeMedicoVisita.Contains("🎂"))
                {
                    visita.IsBirthday = true;
                    visita.NomeMedicoVisita = $"🎂 {visita.NomeMedicoVisita} – Aniversário!";
                }
            }

            return Task.CompletedTask;
        }
    }
}
