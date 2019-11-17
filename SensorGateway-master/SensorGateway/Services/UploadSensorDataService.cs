using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace SensorGateway.Services
{
    public class UploadSensorDataService
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private readonly static string s_connectionString = "HostName=IotHubExTest.azure-devices.net;DeviceId=myDeviceIdTJ12;SharedAccessKey=2WTXJ6ue2oFJ9lLBmTwI0S4mxx3fYxsSpf1Uxvxeb1E=";

        public UploadSensorDataService()
        {
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
        }

        // Async method to send simulated telemetry
        public static async void SendDeviceToCloudMessagesAsync(Object telemetryDataPoint) // Fungerar Object verkligen här?
        {
            // Create JSON message
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            Console.WriteLine(messageString); // Fungerar det?
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            // Send the telemetry message
            await s_deviceClient.SendEventAsync(message);
            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
        }
    }
}
