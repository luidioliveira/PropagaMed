﻿using PropagaMed.Model;
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

            /*Binding para teste de médicos
            medicosPicker.ItemsSource = new List<Medico>
            {
                new Medico {id = 1, nome = "Antônio Carlos de Almeida"},
                new Medico {id = 2, nome = "Lúcio da Silva Oliveira"},
                new Medico {id = 3, nome = "Neide Lima Pinto Oliveira"},
                new Medico {id = 4, nome = "Luidi Lima Oliveira"},
                new Medico {id = 5, nome = "Ana Carla Ricce Telles Corrêa Oliveira"},
                new Medico {id = 6, nome = "Teste de Exibição Acentuada e Com Grande Texto Para Verificar Se O Aplicativo Quebra"}
            };*/

            AlimentaComboMedico();
        }

        protected async void AlimentaComboMedico()
        {
            List<Medico> medicos = await App.Database.GetItemsMedicoAsync();
            List<Visita> visitas = await App.Database.GetItemsVisitaAsync();

            medicosPicker.ItemsSource = medicos;
            listView.ItemsSource = medicos;
            listView2.ItemsSource = visitas;
        }

        private bool validaStringNula(string avalidar)
        {
            if (avalidar is null)
            {
                DisplayAlert("Informação", "Campos necessários estão vazios!", "Ok");
                return true;
            }
            else
                return false;
        }

        private async void cadastrarVisitaClicado(object sender, EventArgs e)
        {
            Visita VisitaASalvar = new Visita();

            VisitaASalvar.idMedicoVisita = MedicoSelecionado.id;
            VisitaASalvar.nomeMedicoVisita = MedicoSelecionado.nome;
            VisitaASalvar.diaVisita = dataVisita.Date;
            VisitaASalvar.horaVisita = horaVisita.Time;
            VisitaASalvar.observacao = obsVisita.Text is null?"":obsVisita.Text.ToString();

            if (validaStringNula(MedicoSelecionado.nome) is false)
            {
                App.Database.SaveItemAsync(VisitaASalvar);
                DisplayAlert("Informação", "Visita para médico(a) " + MedicoSelecionado.nome + " às " + horaVisita.Time.ToString(@"hh\:mm") + " em " + DataSelecionada.ToString("dd/MM/yyyy") + " cadastrada com sucesso!", "Ok");
                await Navigation.PushAsync(new Home());
            }
        }

        private async void cadastrarMedicoClicado(object sender, EventArgs e)
        {
            Medico MedicoASalvar = new Medico();

            MedicoASalvar.nome = nomeMedico.Text;
            MedicoASalvar.especialidade = espMedico.Text;
            MedicoASalvar.localizacao = (localizacaoMedico.SelectedItem is null? "":localizacaoMedico.SelectedItem.ToString());
            MedicoASalvar.endereco = enderecoMedico.Text;
            MedicoASalvar.CEP = CEPMedico.Text;
            MedicoASalvar.aniversario = aniversarioMedico.Date;
            MedicoASalvar.telefone = telefoneMedico.Text;
            MedicoASalvar.celular = celularMedico.Text;
            MedicoASalvar.email = emailMedico.Text;
            MedicoASalvar.diasVisita = (diasVisitaMedicoPicker.SelectedItem is null? "":diasVisitaMedicoPicker.SelectedItem.ToString());
            MedicoASalvar.horariosVisita = (horarioVisitaMedicoPicker.SelectedItem is null? "":horarioVisitaMedicoPicker.SelectedItem.ToString());
            MedicoASalvar.CRM = CRMMedico.Text;

            if (validaStringNula(MedicoASalvar.nome) is false)
            {
                App.Database.SaveItemAsync(MedicoASalvar);
                DisplayAlert("Informação", "Médico(a) " + MedicoASalvar.nome + " cadastrado(a) com sucesso!", "Ok");
                await Navigation.PushAsync(new Home());
            }
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
    }
}