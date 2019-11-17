using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Acr.UserDialogs;
using Xamarin.Forms;
using Android.Content;
using Plugin.Permissions;
using Plugin.CurrentActivity;
using SensorGateway.Messages;
using SensorGateway.ViewModels;
using SensorGateway.Droid.Services;
using SensorGateway.Views;
using static SensorGateway.Droid.Services.BluetoothService;
using Android.Views;

namespace SensorGateway.Droid
{
    //[Activity(Label = "SensorGateway", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    [Activity(Label = "SensorGateway", Theme = "@style/splashscreen", Icon = "@mipmap/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private BluetoothServiceConnection connection;
        private Intent intent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.Window.RequestFeature(WindowFeatures.ActionBar);
            base.SetTheme(Resource.Style.MainTheme);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            WireUpLongRunningTasks();
            UserDialogs.Init(this);

            connection = new BluetoothServiceConnection(this);
            intent = new Intent(this, typeof(BluetoothService));

            StartTSensorBluetoothConnectionService();
            StopTSensorBluetoothConnectionService();
        }

        private void StartTSensorBluetoothConnectionService()
        {
            MessagingCenter.Subscribe<SelectSensorsPage>(this, "StartBluetoothService", message =>
            {
                Console.WriteLine("TSensorBluetoothConnectionService starting");
                StartService(intent);
            });
        }

        private void StopTSensorBluetoothConnectionService()
        {
            MessagingCenter.Subscribe<SelectSensorsPage>(this, "StopBluetoothService", message =>
            {
                Console.WriteLine("TSensorBluetoothConnectionService stopping");
                bool bound = BindService(intent, connection, 0);
                if(bound)
                {
                    connection.StopService();
                }
            });
        }

        public class BluetoothServiceConnection : Java.Lang.Object, IServiceConnection {
            MainActivity mainActivity;
            public bool IsConnected { get; private set; }
            public BluetoothServiceBinder Binder { get; private set; }

            public BluetoothServiceConnection(MainActivity activity)
            {
                IsConnected = false;
                Binder = null;
                mainActivity = activity;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                Binder = service as BluetoothServiceBinder;
                IsConnected = this.Binder != null;
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                IsConnected = false;
                Binder = null;
            }

            public void StopService()
            {
                if(IsConnected)
                {
                    Binder.GetService().StopSelf();
                }
            }
        }


        void WireUpLongRunningTasks()
        {
            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningLocationTaskService));
                StartService(intent);
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningLocationTaskService));
                StopService(intent);
            });
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}