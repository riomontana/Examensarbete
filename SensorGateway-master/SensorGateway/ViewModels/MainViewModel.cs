using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BluetoothLE;
using SensorGateway.Messages;
using SensorGateway.Models;
using SensorGateway.Services;
using SensorGateway.Toast;
using SensorGateway.Views;
using Xamarin.Forms;

namespace SensorGateway.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private SelectSensorsViewModel selectSensorsViewModel;
        private SensorListViewModel sensorListViewModel;
        private readonly LocationService locationService;
        private string strUserLocation;
        private string strLocationOnOff = "Location tracking is off";
        private bool isTrackingLocation;

        public MainViewModel()
        {
            ShowSelectSensorsPageCommand = new Command(ShowSelectSensorsPage);
            ShowSensorListPageCommand = new Command(ShowSensorListPage);

            locationService = new LocationService(this);
            Task.Run(() => locationService.CheckLocationPermissionsAsync());
            Task turnOnBluetoothTask = EnableBluetoothAsync();
        }

        // todo flytta denna
        private async Task EnableBluetoothAsync()
        {
            await Task.Run(() => CrossBleAdapter.Current.SetAdapterState(true));
        }

        public INavigation Navigation { get; set; }
        public string StrUserLocation
        {
            set { SetProperty(ref strUserLocation, value); }
            get { return strUserLocation; }
        }

        public string StrLocationOnOff
        {
            set { SetProperty(ref strLocationOnOff, value); }
            get { return strLocationOnOff; }
        }

        public bool IsTrackingLocation
        {
            get
            {
                return isTrackingLocation;
            }
            set
            {
                isTrackingLocation = value;

                if (isTrackingLocation)
                {
                    Task.Run(() => locationService.StartLocationAsync());
                }
                else
                {
                    var message = new StopLongRunningTaskMessage();
                    MessagingCenter.Send(message, "StopLongRunningTaskMessage");
                    StrLocationOnOff = "Location tracking is off";

                    var toastMessage = new ToastMessage("Location tracking deactivated");
                    toastMessage.ShowToast();
                }
            }
        }

        public Command TrackingLocationCommand { get; }
        public Command ShowSelectSensorsPageCommand { get; }
        public Command ShowSensorListPageCommand { get; }

        // When button clicked, a new page is displayed with the selected sensors in a list
        async void ShowSensorListPage()
        {
            List<Sensor> sensorsInDb = (List<Sensor>)await App.DatabaseContext.GetSensorsAsync();

            if (sensorListViewModel == null)
                sensorListViewModel = new SensorListViewModel();
                
            sensorListViewModel.CheckActiveSensors(sensorsInDb);

            await Navigation.PushAsync(new SensorListPage()
            {
                BindingContext = sensorListViewModel
            });
        }

        // When button clicked, a new page is displayed with the available sensors that can be selected
        async void ShowSelectSensorsPage()
        {
            List<Sensor> sensorsInDb = (List<Sensor>) await App.DatabaseContext.GetSensorsAsync();          

            if (selectSensorsViewModel == null)
                selectSensorsViewModel = new SelectSensorsViewModel();

            selectSensorsViewModel.CheckActiveSensors(sensorsInDb);

            await Navigation.PushAsync(new SelectSensorsPage()
            {
                BindingContext = selectSensorsViewModel
            });
        }
    }
}
