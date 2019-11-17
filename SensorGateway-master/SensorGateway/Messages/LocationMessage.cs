using System;
namespace SensorGateway.Messages
{
    public class LatLon
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }

    public class LocationMessage
    {
        public string Message { get; set; }
    }
}
