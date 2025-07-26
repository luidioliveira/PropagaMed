using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using Plugin.FilePicker;
using Xamarin.Essentials;
using brevo_csharp.Api;
using brevo_csharp.Model;

namespace PropagaMed
{
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();

            var initialize = System.Threading.Tasks.Task.Run(() => {
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
                        return true;
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

                            await App.Database.CloseConnections();
                            File.Delete(basePath);
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
                var dbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DBPropagaMed.db3");
                var zipFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BkpPropagaMed.zip");
                var configItems = Config.GetConfigItems();

                try
                {
                    //Criando ZIP
                    using (var zipStream = File.Create(zipFilePath))
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false))
                    {
                        var dbEntry = archive.CreateEntry("DBPropagaMed.db3");

                        using (var entryStream = dbEntry.Open())
                        using (var fileStream = File.OpenRead(dbFilePath))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }

                    brevo_csharp.Client.Configuration.Default.AddApiKey("api-key", configItems.BrevoApiKey);
                    var apiInstance = new TransactionalEmailsApi();

                    var sendSmtpEmail = new SendSmtpEmail
                    {
                        Sender = new SendSmtpEmailSender("PropagaMed", configItems.MailFrom),
                        To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo("luidi.lima@poli.ufrj.br", "Usuário PropagaMed") },
                        Subject = $"PropagaMed - {userData.Mail} - Backup de Banco de Dados - {DeviceInfo.Model} - {DateTime.Now:dd/MM/yyyy}",
                        TextContent = $"{userData.Mail} - Backup do banco de dados anexado - {DeviceInfo.Model}",
                        HtmlContent = $"<strong>{userData.Mail} - Backup do banco de dados anexado - {DeviceInfo.Model}</strong>",
                        Attachment = new List<SendSmtpEmailAttachment>
                        {
                            new SendSmtpEmailAttachment
                            {
                                Name = "BkpPropagaMed.zip",
                                Content = File.ReadAllBytes(zipFilePath)
                            }
                        }
                    };

                    var response = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
                    await DisplayAlert("Informação", $"Backup enviado com sucesso em {DateTime.Now:dd/MM/yyyy HH:mm}", "Ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Falha no envio de backup: {ex.Message}", "Ok");
                }
                finally
                {
                    if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                }
            }
        }

        [ContentProperty(nameof(Source))]
        public class ImageResourceExtension : IMarkupExtension
        {
            public string Source { get; set; }

            public object ProvideValue(IServiceProvider serviceProvider)
            {
                if (Source == null) return null;
                return ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);
            }
        }
    }
}