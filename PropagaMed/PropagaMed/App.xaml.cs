using PropagaMed.Dal;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PropagaMed
{
    public partial class App : Application
	{
		static ItemDatabase database;

		public App ()
		{
			InitializeComponent();

            MainPage = new NavigationPage(new Login());
		}

		public static ItemDatabase Database
		{
			get
			{
				if (database == null)
				{
					database = new ItemDatabase();
				}
				return database;
			}
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
