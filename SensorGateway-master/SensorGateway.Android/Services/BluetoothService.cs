using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Plugin.BluetoothLE;
using SensorGateway.Database;
using SensorGateway.Messages;
using SensorGateway.Models;
using SensorGateway.Services;
using Xamarin.Forms;

namespace SensorGateway.Droid.Services
{
    [Service]
    [IntentFilter(new System.String[] { "com.companyname.BluetoothService" })]
    public class BluetoothService : Service
    {
        IDevice device;
        private DatabaseContext dbContext;
        private List<Sensor> sensors;
        private UploadSensorDataService UploadSensorDataService;
        private IBinder binder;
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            binder = new BluetoothServiceBinder(this);
            var dbPath = DependencyService.Get<IFileHelper>().GetLocalFilePath("SensorGateway.db");
            dbContext = new DatabaseContext(dbPath);
            GetSensors(dbContext);

            Thread t = new BluetoothSensorThread(sensors);
            t.Start();

            UploadSensorDataService = new UploadSensorDataService();

            return StartCommandResult.Sticky;
        }

        private async void GetSensors(DatabaseContext dbContext)
        {
            sensors = (List<Sensor>)await dbContext.GetSensorsAsync();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            return binder;
        }

        public class BluetoothSensorThread : Thread
        {
            private IDisposable scanner;
            private List<Sensor> sensors;
            private double lat = 0;
            private double lon = 0;
            private TSensorBluetoothService tSensorBluetoothService;

            public BluetoothSensorThread(List<Sensor> sensors)
            {
                this.sensors = sensors;
                tSensorBluetoothService = new TSensorBluetoothService(sensors);
                ListenForLocation();
            }

            public void ListenForLocation()
            {
                MessagingCenter.Subscribe<LatLon>(this, "Location", latlon =>
                {
                    lat = latlon.lat;
                    lon = latlon.lon;
                });
            }

            public override void Run()
            {
                CrossBleAdapter.Current.StopScan();
                while (CrossBleAdapter.Current.IsScanning)
                {
                    Console.WriteLine("Scan is already active, waiting 1s.T");
                    Sleep(3000);
                }

                while (true)
                {
                    ScanSensors();
                }
            }

            public void ScanSensors()
            {
                scanner = CrossBleAdapter.Current.Scan().Subscribe(scanResult =>
                {
                    foreach (Sensor s in sensors)
                    {
                        if (!s.Uuid.Equals(scanResult.Device.Uuid.ToString()))
                        {
                            continue;
                        }

                        if (s.Name.Equals("T-Sensor"))
                        {
                            tSensorBluetoothService.ReadSensorData(scanResult, s, lat, lon);
                            continue;
                        }
                    }
                });

                Sleep(56000);
                scanner.Dispose();
            }
        }

        public class BluetoothServiceBinder : Binder
        {
            private BluetoothService service;

            public BluetoothServiceBinder(BluetoothService _service)
            {
                service = _service;
            }

            public BluetoothService GetService()
            {
                return service;
            }
        }
    }
}