using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PropagaMed
{
	public partial class Login : ContentPage
	{
        public Login()
        {
            InitializeComponent();

            PropagaMedLogo.Opacity = 0;
            PropagaMedLogo.FadeTo(1, 4000);
        }

        private async void EntrarClicado(object sender, EventArgs e)
        {
            if ((login.Text is null?"": login.Text.ToUpper()) == "LUCIOOLIVEIRA" && password.Text == "2604")
            {
                await Navigation.PushAsync(new Home());
                login.Text = "";
                password.Text = "";
            }
            else
            {
                DisplayAlert("Informação", "Login e/ou senha incorreto(s)", "Ok");
            }
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
