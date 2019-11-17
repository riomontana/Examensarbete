using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
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
    [IntentFilter(new System.String[] { "com.companyname.TSensorBluetoothService" })]
    public class TSensorBluetoothService
    {
        private long[] sensorReadings;
        private List<Sensor> sensors;

        public TSensorBluetoothService(List<Sensor> sensors)
        {
            this.sensors = sensors;
            sensorReadings = new long[sensors.Count];
        }

        public void ReadSensorData(IScanResult scanResult, Sensor s, double lat, double lon)
        {
            if(!IsTimeToReadSensor(s))
            {
                return;
            }

            sensorReadings[sensors.IndexOf(s)] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (scanResult.AdvertisementData.ServiceData.Count == 0)
            {
                return;
            }
            byte[] serviceData = GetServiceDataBytes(scanResult);

            TSensorData sensorData = TSensorService.ParseSensorData(serviceData);
            if (sensorData == null)
            {
                return;
            }
            var date = DateTime.Now.ToString();
            Console.WriteLine(
                string.Format(
                    "Name: {0} - UUID: {1} - Temperature: {2} - Pressure: {3} - Altitude: {4} - Lat: {5} - Lon: {6}",
                    s.Name,
                    s.Uuid,
                    sensorData.Temperature,
                    sensorData.Pressure,
                    sensorData.Altitude,
                    lat,
                    lon
                )
            );

            var telemetryDataPoint = new
            {
                uuid = s.Uuid,
                name = s.Name,
                temperature = sensorData.Temperature,
                pressure = sensorData.Pressure,
                altitude = sensorData.Altitude,
                battery = sensorData.Battery,
                lat,
                lon,
                date,
            };

            UploadSensorDataService.SendDeviceToCloudMessagesAsync(telemetryDataPoint); // Kolla så Object faktiskt fungerar
        }

        public byte[] GetServiceDataBytes(IScanResult scanResult)
        {
            byte[] serviceData = new byte[scanResult.AdvertisementData.ServiceData[0].Length];

            if (scanResult.AdvertisementData.ServiceData[0].Length > 0)
            {
                for (int j = 0; j < scanResult.AdvertisementData.ServiceData[0].Length; j++)
                {
                    serviceData[j] = scanResult.AdvertisementData.ServiceData[0][j];
                }
            }

            return serviceData;
        }

        public bool IsTimeToReadSensor(Sensor s)
        {
            long nowInMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long lastTimeSensorRead = sensorReadings[sensors.IndexOf(s)];
            long interval = s.DateAndInterval.Interval * 60000;

            if (nowInMilliseconds - (lastTimeSensorRead + interval) <= 0) return false;

            DateTime today = DateTime.Now;
            DateTime fromDate = s.DateAndInterval.FromDate;
            DateTime toDate = s.DateAndInterval.ToDate.AddHours(23).AddMinutes(59).AddSeconds(59);

            Console.WriteLine("ToDate: " + toDate.ToString());

            if (today < fromDate || today > toDate) return false;

            return true;
        }
    }
}