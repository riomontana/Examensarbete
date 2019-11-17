using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SensorGateway.Models;

namespace SensorGateway.Services
{
    public interface IDataStoreSensor<Sensor>
    {
        Task<Sensor> GetSensorAsync(int id);

        Task<IEnumerable<Sensor>> GetSensorsAsync(bool forceRefresh = false);

        Task<bool> AddSensorAsync(Sensor sensor);

        Task<bool> UpdateSensorAsync(Sensor sensor);

        Task<bool> DeleteSensorAsync(int id);
    }
}
