using System;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PropagaMed
{
    public partial class Login : ContentPage
	{
        public Login(bool logado)
        {
            /*
            if (!logado)
            {
                InitializeComponent();

                PropagaMedLogo.Opacity = 0;
                PropagaMedLogo.FadeTo(1, 4000);
            }
            else
            {
                Navigation.PopAsync();
                Navigation.PushAsync(new Home());
            }
            */
            InitializeComponent();

            Task initialize  = Task.Run(() => {
                                                PropagaMedLogo.Opacity = 0;
                                                PropagaMedLogo.FadeTo(1, 4000);
                                              });

            initialize.Wait();
        }

        private async void EntrarClicado(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Home());

            /*
            if ((login.Text is null?"": login.Text.ToUpper()) == "LUCIOOLIVEIRA" && password.Text == "2604")
            {
                await Navigation.PopAsync();
                await Navigation.PushAsync(new Home());
            }
            else
            {
                DisplayAlert("Informação", "Login e/ou senha incorreto(s)", "Ok");
            }
            */
        }

        private void InfoClicado(object sender, EventArgs e)
        {
            DisplayAlert("Informação", "App desenvolvido por Luidi Oliveira © Copyright", "Ok");
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

                // Do your translation lookup here, using whatever method you require
                var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);

                return imageSource;
            }
        }
    }
}
