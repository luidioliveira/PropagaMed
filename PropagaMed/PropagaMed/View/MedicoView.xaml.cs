using System;
using PropagaMed.Model;
using PropagaMed.Utils;
using Xamarin.Forms;

namespace PropagaMed.View
{
    public partial class MedicoView : ContentPage
    {
        readonly int medicoId = default;
        private Medico _medicoAtual;

        public MedicoView(Medico medico)
        {
            InitializeComponent();

            _medicoAtual = medico;
            medicoId = medico.Id;

            nomeMedico.Text = medico.Nome;
            espMedico.Text = medico.Especialidade;
            localizacaoMedico.SelectedItem = medico.Localizacao;
            enderecoMedico.Text = medico.Endereco;
            CEPMedico.Text = medico.CEP;
            aniversarioMedico.Date = medico.Aniversario;
            secretaria.Text = medico.Secretaria;
            telefoneMedico.Text = medico.Telefone;
            celularMedico.Text = medico.Celular;
            emailMedico.Text = medico.Email;
            CRMMedico.Text = medico.CRM;
            obsMedico.Text = medico.Observacao;

            // Tipos preferenciais de visita
            presencialTipoVisita.IsChecked = medico.TiposVisita is not null && medico.TiposVisita.Contains("Presencial");
            onlineTipoVisita.IsChecked = medico.TiposVisita is not null && medico.TiposVisita.Contains("Online");

            // Dias preferenciais de visita
            monday.IsChecked = medico.DiasVisita is not null && medico.DiasVisita.Contains("Segunda");
            tuesday.IsChecked = medico.DiasVisita is not null && medico.DiasVisita.Contains("Terça");
            wednesday.IsChecked = medico.DiasVisita is not null && medico.DiasVisita.Contains("Quarta");
            thursday.IsChecked = medico.DiasVisita is not null && medico.DiasVisita.Contains("Quinta");
            friday.IsChecked = medico.DiasVisita is not null && medico.DiasVisita.Contains("Sexta");

            // Turnos preferenciais de visita
            manhaMonday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("ManhãSegunda");
            tardeMonday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("TardeSegunda");
            noiteMonday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("NoiteSegunda");
            manhaTuesday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("ManhãTerça");
            tardeTuesday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("TardeTerça");
            noiteTuesday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("NoiteTerça");
            manhaWednesday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("ManhãQuarta");
            tardeWednesday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("TardeQuarta");
            noiteWednesday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("NoiteQuarta");
            manhaThursday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("ManhãQuinta");
            tardeThursday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("TardeQuinta");
            noiteThursday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("NoiteQuinta");
            manhaFriday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("ManhãSexta");
            tardeFriday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("TardeSexta");
            noiteFriday.IsChecked = medico.HorariosVisita is not null && medico.HorariosVisita.Contains("NoiteSexta");
        }

        private async void ExportarCartaoClicado(object sender, EventArgs e)
        {
            try
            {
                btnExportarCartao.Text = "Gerando cartão...";
                btnExportarCartao.IsEnabled = false;

                var medicoParaExport = new Medico
                {
                    Id = medicoId,
                    Nome = nomeMedico.Text,
                    Especialidade = espMedico.Text,
                    Localizacao = localizacaoMedico.SelectedItem?.ToString() ?? "",
                    Endereco = enderecoMedico.Text,
                    CEP = CEPMedico.Text,
                    Aniversario = aniversarioMedico.Date,
                    Secretaria = secretaria.Text,
                    Telefone = telefoneMedico.Text,
                    Celular = celularMedico.Text,
                    Email = emailMedico.Text,
                    CRM = CRMMedico.Text,
                    DiasVisita = DiasETurnosVisitaHelper.FormatarDias(_medicoAtual.DiasVisita),
                    HorariosVisita = DiasETurnosVisitaHelper.FormatarTurnos(_medicoAtual.HorariosVisita),
                    Observacao = obsMedico.Text
                };

                await CardExportService.ExportCardAsync(medicoParaExport);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível gerar o cartão:\n{ex.Message}", "Ok");
            }
            finally
            {
                btnExportarCartao.IsEnabled = true;
                btnExportarCartao.Text = "Exportar Cartão";
            }
        }

        private async void AtualizarMedicoClicado(object sender, EventArgs e)
        {
            var returnPage = new Home(otherView: "verMedicos");
            var medicoAAtualizar = new Medico { Id = medicoId };

            string tiposVisitaSelecionados = string.Empty;
            string diasVisitaSelecionados = string.Empty;
            string horariosVisitaSelecionados = string.Empty;

            if (presencialTipoVisita.IsChecked)
                tiposVisitaSelecionados += "Presencial";
            if (onlineTipoVisita.IsChecked)
                tiposVisitaSelecionados += string.IsNullOrEmpty(tiposVisitaSelecionados) ? "Online" : "e Online";

            if (monday.IsChecked)
            {
                diasVisitaSelecionados += "Segunda";
                if (manhaMonday.IsChecked) horariosVisitaSelecionados += "ManhãSegunda";
                if (tardeMonday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeSegunda" : " e TardeSegunda";
                if (noiteMonday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteSegunda" : " e NoiteSegunda";
            }
            if (tuesday.IsChecked)
            {
                diasVisitaSelecionados += string.IsNullOrEmpty(diasVisitaSelecionados) ? "Terça" : " e Terça";
                if (manhaTuesday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "ManhãTerça" : " e ManhãTerça";
                if (tardeTuesday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeTerça" : " e TardeTerça";
                if (noiteTuesday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteTerça" : " e NoiteTerça";
            }
            if (wednesday.IsChecked)
            {
                diasVisitaSelecionados += string.IsNullOrEmpty(diasVisitaSelecionados) ? "Quarta" : " e Quarta";
                if (manhaWednesday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "ManhãQuarta" : " e ManhãQuarta";
                if (tardeWednesday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeQuarta" : " e TardeQuarta";
                if (noiteWednesday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteQuarta" : " e NoiteQuarta";
            }
            if (thursday.IsChecked)
            {
                diasVisitaSelecionados += string.IsNullOrEmpty(diasVisitaSelecionados) ? "Quinta" : " e Quinta";
                if (manhaThursday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "ManhãQuinta" : " e ManhãQuinta";
                if (tardeThursday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeQuinta" : " e TardeQuinta";
                if (noiteThursday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteQuinta" : " e NoiteQuinta";
            }
            if (friday.IsChecked)
            {
                diasVisitaSelecionados += string.IsNullOrEmpty(diasVisitaSelecionados) ? "Sexta" : " e Sexta";
                if (manhaFriday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "ManhãSexta" : " e ManhãSexta";
                if (tardeFriday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "TardeSexta" : " e TardeSexta";
                if (noiteFriday.IsChecked) horariosVisitaSelecionados += string.IsNullOrEmpty(horariosVisitaSelecionados) ? "NoiteSexta" : " e NoiteSexta";
            }

            medicoAAtualizar.Nome = nomeMedico.Text;
            medicoAAtualizar.Especialidade = espMedico.Text;
            medicoAAtualizar.Localizacao = localizacaoMedico.SelectedItem?.ToString() ?? "";
            medicoAAtualizar.Endereco = enderecoMedico.Text;
            medicoAAtualizar.CEP = CEPMedico.Text;
            medicoAAtualizar.Aniversario = aniversarioMedico.Date;
            medicoAAtualizar.Secretaria = secretaria.Text;
            medicoAAtualizar.Telefone = telefoneMedico.Text;
            medicoAAtualizar.Celular = celularMedico.Text;
            medicoAAtualizar.Email = emailMedico.Text;
            medicoAAtualizar.TiposVisita = tiposVisitaSelecionados;
            medicoAAtualizar.DiasVisita = diasVisitaSelecionados;
            medicoAAtualizar.HorariosVisita = horariosVisitaSelecionados;
            medicoAAtualizar.CRM = CRMMedico.Text;
            medicoAAtualizar.Observacao = obsMedico.Text;

            _medicoAtual = medicoAAtualizar;

            if (!string.IsNullOrEmpty(medicoAAtualizar.Nome))
            {
                await App.Database.UpdateItemAsync(medicoAAtualizar);
                await DisplayAlert("Informação",
                    $"Cadastro do(a) Médico(a) {medicoAAtualizar.Nome} atualizado com sucesso", "Ok");
                returnPage.AlimentaMedicosEVisitas();
                await Navigation.PushAsync(returnPage);
            }
            else
                await DisplayAlert("Informação", "Campos necessários estão vazios", "Ok");
        }
    }
}