using SensorGateway.ViewModels;
using Xamarin.Forms;

namespace SensorGateway.Views
{
    /// <summary>
    /// Main Gui page of the application
    /// </summary>
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(this, "StartBluetoothService");
        }
    }
}

