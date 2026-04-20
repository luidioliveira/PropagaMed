using PropagaMed.Dal;
using PropagaMed.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PropagaMed
{
    public partial class App : Application
    {
        static ItemDatabase database;

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new Login());
        }

        public static ItemDatabase Database
        {
            get
            {
                database ??= new();

                return database;
            }
        }

        protected override void OnStart()
        {
            _ = BirthdayNotificationService.CheckTodayBirthdaysAsync();
        }

        protected override void OnSleep() { }

        protected override void OnResume()
        {
            _ = BirthdayNotificationService.CheckTodayBirthdaysAsync();
        }
    }
}