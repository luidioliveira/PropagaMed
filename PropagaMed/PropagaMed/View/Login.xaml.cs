using System;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using SendGrid;
using SendGrid.Helpers.Mail;
using Plugin.FilePicker;
using Xamarin.Essentials;

namespace PropagaMed
{
    public partial class Login : ContentPage
	{
        public Login()
        {
            InitializeComponent();

            Task initialize  = Task.Run(() => {
                                                PropagaMedLogo.Opacity = 0;
                                                PropagaMedLogo.FadeTo(1, 4000);
                                              });

            initialize.Wait();
        }

        private bool ValidarUsuario(bool isMaster = false)
        {
            var configItems = Config.GetConfigItems();

            if (!isMaster)
            {
                var deserializedUsersData = JsonConvert.DeserializeObject<List<UserData>>(configItems.UsersData);

                foreach (var userData in deserializedUsersData)
                {
                    if (userData.Mail.Equals(emailUsuario.Text) && userData.Pin.Equals(pinUsuario.Text))
                    {
                        return true;
                    }
                }
            }
            else
            {
                var deserializedMasterUser = JsonConvert.DeserializeObject<UserData>(configItems.MasterUser);

                if (emailUsuario.Text.Equals(deserializedMasterUser.Mail) && pinUsuario.Text.Equals(deserializedMasterUser.Pin))
                    return true;
            }

            return false;
        }

        private async void EntrarClicadoAsync(object sender, EventArgs e)
        {
            if (ValidarUsuario())
            {
                try
                {
                    var userDataBase = App.Database.GetItemsUserDataAsync().Result.Find(u => u.Mail.Equals(emailUsuario.Text));

                    if (userDataBase is not null)
                    {
                        userDataBase.CountLogin++;
                        
                        CheckDatabaseBkp(userDataBase);

                        if (userDataBase.CountLogin >= 4)
                            userDataBase.CountLogin = 0;

                        await App.Database.UpdateItemAsync(userDataBase);
                    }
                    else
                    { 
                        await App.Database.SaveItemAsync(new UserData() { Mail = emailUsuario.Text, Pin = pinUsuario.Text, CountLogin = 0 });
                        await DisplayAlert("Informação", $"Novo usuário {emailUsuario.Text} salvo, avalie com a TI se não há backup do seu banco de dados a recuperar", "Ok");
                    }
                }
                catch 
                {
                    await DisplayAlert("Atenção", $"Tente o login com o usuário {emailUsuario.Text} novamente mais tarde", "Ok");
                }
                finally
                {
                    await Navigation.PushAsync(new Home());
                }
            }
            else
                await DisplayAlert("Informação", $"Usuário {emailUsuario.Text} sem acesso permitido ou PIN incorreto", "Ok");
        }

        private async void RecoveryBDClicadoAsync(object sender, EventArgs e)
        {
            if (ValidarUsuario(true))
            {
                var confirmDelete = await DisplayAlert("Atenção", $"Isso deletará sua base de dados para que coloque a nova. Deseja prosseguir?", "Sim", "Não");

                if (confirmDelete)
                {
                    try
                    {
                        var file = await FilePicker.PickAsync(new() { PickerTitle = "Selecione o banco de dados" });

                        if (file == null || !file.FileName.Contains(".db3"))
                            await DisplayAlert("Informação", $"Selecione um banco de dados válido", "Ok");
                        else
                        { 
                            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DBPropagaMed.db3");

                            //Deletando banco de dados antigo
                            await App.Database.CloseConnections();
                            File.Delete(basePath);

                            //Escrevendo backup
                            File.WriteAllBytes(basePath, File.ReadAllBytes(file.FullPath));

                            await DisplayAlert("Informação", $"Novo banco de dados inputado com sucesso", "Ok");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Erro", $"Erro ao salvar novo banco de dados", "Ok");
                        throw ex;
                    }
                }
            }
            else
            {
                await DisplayAlert("Informação", $"Usuário {emailUsuario.Text} sem acesso permitido ou PIN incorreto", "Ok");
            }
        }

        private async void CheckDatabaseBkp(UserData userData)
        {
            if (userData.CountLogin == 4)
            {
                var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DBPropagaMed.db3");

                //Envio de e-mail
                var configItems = Config.GetConfigItems();

                var apiKey = configItems.SendGridApiKey;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(configItems.MailFrom, "PropagaMed");
                var subject = $"PropagaMed - {userData.Mail} - Backup de Banco de Dados - {DeviceInfo.Model} - {DateTime.Now:dd/MM/yyyy}";
                var to = new EmailAddress("luidi.lima@poli.ufrj.br", "Usuário PropagaMed");
                var plainTextContent = $"{userData.Mail} - Segue anexo o arquivo de backup de banco de dados - {DeviceInfo.Model} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                var htmlContent = $"<strong>{userData.Mail} - Segue anexo o arquivo de backup de banco de dados - {DeviceInfo.Model} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                //Banco de dados como anexo
                var fileBytes = File.ReadAllBytes(basePath);
                var fileBase64 = Convert.ToBase64String(fileBytes);
                msg.AddAttachment("DBPropagaMed.db3", fileBase64);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Informação", $"Backup realizado com sucesso em {DateTime.Now:dd/MM/yyyy HH:mm}", "Ok");
                }
                else
                    await DisplayAlert("Erro", $"Falha no backup e haverá nova tentativa mais tarde.", "Ok");
            }
        }

        [ContentProperty(nameof(Source))]
        public class ImageResourceExtension : IMarkupExtension
        {
            public string Source { get; set; }

            public object ProvideValue(IServiceProvider serviceProvider)
            {
                if (Source == null)
                {
                    return null;
                }

                var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);

                return imageSource;
            }
        }
    }
}
