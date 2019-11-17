using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SensorGateway.Views;
using System.Diagnostics;
using SensorGateway.ViewModels;
using SensorGateway.Database;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SensorGateway
{
    public partial class App : Application
    {
        static DatabaseContext dbContext;

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage()
            {
                BindingContext = new MainViewModel()
            });
        }

        public static DatabaseContext DatabaseContext
        {
            get
            {
                if (dbContext == null)
                {
                    // Changes here by @cwrea for adaptation to EF Core.
                    var databasePath = DependencyService.Get<IFileHelper>().GetLocalFilePath("SensorGateway.db");
                    Debug.WriteLine("databasePath: " + databasePath);
                    dbContext = DatabaseContext.Create(databasePath);
                }
                return dbContext;
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
