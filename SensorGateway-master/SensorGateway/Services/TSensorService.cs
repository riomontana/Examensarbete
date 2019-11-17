using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensorGateway.Services
{
    public class TSensorService
    {

        public static TSensorData ParseSensorData(byte[] bytes)
        {
            if (bytes.Count() < 12) // || bytes[1] != 22)
            {
                return null;
            }

            float temperature = ParseSensorData(bytes[2], bytes[3], bytes[4]) / 100.0f;
            float pressure = ParseSensorData(bytes[5], bytes[6], bytes[7]) / 100.0f;
            float altitude = ParseSensorData(bytes[8], bytes[9], bytes[10]) / 100.0f;
            int battery = bytes[11] & 0xFF;

            return new TSensorData(temperature, pressure, altitude, battery);
        }

        private static int ParseSensorData(byte b1, byte b2, byte b3)
        {
            if((b1 & 0x8) == 0)
            {
                return (b1 & 0xF) << 16 | (b2 & 0xFF) << 8 | b3 & 0xFF;
            }

            int i = b1 << 16 | b2 << 8 | b3;

            List<Int32> bits = new List<int>();

            for(int j = 0; j < 20; j++)
            {
                bits.Add((0x1 & i >> j) == 0 ? 1 : 0);
            }

            i = 0;

            for(int j = 0; j < 20; j++)
            {
                i = (int)(i + Math.Pow(2.0D, j) * bits[j]);
            }

            i++;

            i = 0 - i;

            return i;
        }
    }

    public class TSensorData // TEMPORARY, METHOD ABOVE SHOULD PROBABLY RETURN SAME SENSOR OBJECT AS EVERYONE ELSE OR AT LEAST HAVE THE SAME PARENT CLASS
    {
        public float Temperature { get; }
        public float Pressure { get; }
        public float Altitude { get; }
        public int Battery { get; }

        public TSensorData(float temperature, float pressure, float altitude, int battery)
        {
            Temperature = temperature;
            Pressure = pressure;
            Altitude = altitude;
            Battery = battery;
        }
    }
}
