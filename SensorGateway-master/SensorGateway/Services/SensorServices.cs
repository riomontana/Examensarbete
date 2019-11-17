using System;
using System.Linq;
using System.Collections.ObjectModel;
using SensorGateway.Models;
using SensorGateway.ViewModels;
using Plugin.BluetoothLE;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using SensorGateway.Views;
using SensorGateway.Database;

namespace SensorGateway.Services
{
    public class SensorServices
    {
        private IDisposable scanner;
        private readonly SelectSensorsViewModel selectSensorsViewModel;
        private DatabaseContext dbContext;
        private List<Sensor> existingSensors;

        public List<IDevice> BleDevices { get; } = new List<IDevice>();

        public SensorServices(SelectSensorsViewModel selectSensorsViewModel)
        {
            this.selectSensorsViewModel = selectSensorsViewModel;

            var dbPath = DependencyService.Get<IFileHelper>().GetLocalFilePath("SensorGateway.db");
            dbContext = new DatabaseContext(dbPath);
            GetSensors(dbContext);

            MessagingCenter.Subscribe<SelectSensorsPage>(this, "StopSensorService", message =>
            {
                scanner?.Dispose();
            });
        }

        private async void GetSensors(DatabaseContext dbContext)
        {
            existingSensors = (List<Sensor>)await dbContext.GetSensorsAsync();
        }

        // Mock-up data TODO remove later
        public ObservableCollection<Sensor> GetSensors()
        {
            var sensorList = new ObservableCollection<Sensor>
            {
                new Sensor("Accelerometer", "f0ccadc3-fe2c-45dc-a89c-3862a8b53940", false, null, new byte[0]),
                new Sensor("Gyroscope", "d0eb2f11-3eab-4f4b-b229-200c574a3b5f", false, null, new byte[0]),
                new Sensor("Humidity sensor", "cebc8821-c8a9-44cf-9cba-1a1b2409dc90", false, null, new byte[0])
            };

            return sensorList;
        }

        public async void RefreshSensors()
        {
            while(CrossBleAdapter.Current.IsScanning)
            {
                Console.WriteLine("Scan is already active, waiting 1s.");
                await Task.Delay(1000);
            }

            Console.WriteLine("Starting search for sensors");

            scanner = CrossBleAdapter.Current.Scan().Subscribe(scanResult =>
            {
                ObservableCollection<Sensor> sensorList = selectSensorsViewModel.SensorList;

                foreach (Sensor s in sensorList)
                {
                    if (s.Uuid.Equals(scanResult.Device.Uuid.ToString()))
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(scanResult.Device.Name))
                    {
                        return;
                    }
                }

                byte[] serviceData = new byte[scanResult.AdvertisementData.ServiceData.Count];

                if (scanResult.AdvertisementData.ServiceData.Count > 0)
                {
                    for (int i = 0; i < scanResult.AdvertisementData.ServiceData.Count(); i++)
                    {
                        serviceData[i] = scanResult.AdvertisementData.ServiceData[0][i];
                    }
                }

                Sensor sensor = new Sensor(scanResult.Device.Name, scanResult.Device.Uuid.ToString(), false, null, serviceData);
                Console.WriteLine("Sensor found. Name is: " + sensor.Name);

                foreach (Sensor es in existingSensors)
                {
                    if(es.Uuid == scanResult.Device.Uuid.ToString())
                    {
                        sensor = es;
                    }
                }

                selectSensorsViewModel.SensorList.Add(sensor);
                BleDevices.Add(scanResult.Device);
            });
        }
    }
}
