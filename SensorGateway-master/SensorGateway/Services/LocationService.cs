using System;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using SensorGateway.Messages;
using SensorGateway.Toast;
using SensorGateway.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SensorGateway.Services
{
    public class LocationService
    {

        private readonly MainViewModel mainViewModel;
        public double lat;
        public double lon;

        public LocationService()
        { }

        public LocationService(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            HandleReceivedMessages();
        }

        public async Task StartLocationAsync()
        {
            var message = new StartLongRunningTaskMessage();
            MessagingCenter.Send(message, "StartLongRunningTaskMessage");

            var toastMessage = new ToastMessage("Location tracking activated");
            toastMessage.ShowToast();
            mainViewModel.StrLocationOnOff = "Location tracking is on";
        }

        // Checks location permission
        public async Task CheckLocationPermissionsAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await UserDialogs.Instance.ConfirmAsync("Allow location tracking?", "Press OK to allow location tracking", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                {
                    // Permission was granted
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await UserDialogs.Instance.AlertAsync("Location Denied", "Can not continue, try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                mainViewModel.StrUserLocation = "Error: " + ex;
            }
        }

        // Receives messages from the location service and sets a label with location data
        void HandleReceivedMessages()
        {
            MessagingCenter.Subscribe<LocationMessage>(this, "LocationMessage", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    mainViewModel.StrUserLocation = message.Message;
                });
            });
        }


        public async Task GetUserLocationAsync(CancellationToken token)
        {

            await Task.Run(async () => {

                for (long i = 0; i < long.MaxValue; i++)
                {
                    token.ThrowIfCancellationRequested();

                    await Task.Delay(1000);

                    try
                    {
                        var location = await Geolocation.GetLastKnownLocationAsync();

                        if (location != null)
                        {
                            lat = location.Latitude;
                            lon = location.Longitude;
                            var latlon = new LatLon
                            {
                                lat = location.Latitude,
                                lon = location.Longitude
                            };

                            var message = new LocationMessage
                            {
                                Message = String.Format("Current location:\n" +
                                            "Latitude: {0} \n" +
                                            "Longitude: {1} \n" +
                                            "Altitude: {2:0.00} \n" +
                                            "Service running: " + i + " seconds", location.Latitude, location.Longitude, location.Altitude)
                            };


                            Device.BeginInvokeOnMainThread(() => {
                                MessagingCenter.Send<LatLon>(latlon, "Location");
                                MessagingCenter.Send<LocationMessage>(message, "LocationMessage");
                            });

                        }
                    }
                    catch (FeatureNotSupportedException fnsEx)
                    {
                        // Handle not supported on device exception
                        Console.WriteLine(fnsEx);
                    }
                    catch (FeatureNotEnabledException fneEx)
                    {
                        // Handle not enabled on device exception
                        Console.WriteLine(fneEx);
                    }
                    catch (PermissionException pEx)
                    {
                        // Handle permission exception
                        Console.WriteLine(pEx);
                    }
                    catch (Exception ex)
                    {
                        // Unable to get location
                        Console.WriteLine(ex);
                    }
                }
            }, token);


        }
    }
}
