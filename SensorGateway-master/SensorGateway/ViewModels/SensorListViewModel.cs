using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SensorGateway.Database;
using SensorGateway.Models;
using Xamarin.Forms;

namespace SensorGateway.ViewModels
{
    public class SensorListViewModel : ViewModelBase
    {
        private ObservableCollection<Sensor> activeSensorsList;

        public SensorListViewModel()
        {
        }

        public INavigation Navigation { get; set; }

        public ObservableCollection<Sensor> ActiveSensorsList
        {
            get { return activeSensorsList; }
            set { SetProperty(ref activeSensorsList, value); }
        }

        public void CheckActiveSensors(List<Sensor> sensorsInDb)
        {
            activeSensorsList = null;

            if (sensorsInDb == null)
            {
                activeSensorsList = new ObservableCollection<Sensor>
                {
                    new Sensor("No sensors active")
                };
            }
            else if (sensorsInDb.Count == 0)
            {
                activeSensorsList = new ObservableCollection<Sensor>
                {
                    new Sensor("No sensors active")
                };
                //activeSensorsList = new ObservableCollection<Sensor>();
                //activeSensorsList.Add(new Sensor(1, "No sensors active", "", new byte[0], false)); // TODO! Fix byte array to be correct
            }
            else
            {
                activeSensorsList = new ObservableCollection<Sensor>(sensorsInDb);
            }
        }
    }
}
