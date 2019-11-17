using System;
using SensorGateway.Models;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SensorGateway.Toast;

namespace SensorGateway.ViewModels
{
    public class DateAndIntervalViewModel : ViewModelBase
    {
        private DateAndInterval dateAndInterval;
        private DateTime minimumDate = DateTime.Today;
        private readonly bool updateSensor;
        private readonly Sensor sensor;
        private SelectSensorsViewModel parentViewModel;
        private ObservableCollection<Sensor> sensorList;
        private readonly bool activateAllSensors = false;

        // Called when sensor is activated
        public DateAndIntervalViewModel(Sensor sensor, SelectSensorsViewModel parentViewModel)
        {
            updateSensor = false;
            this.sensor = sensor;
            SaveDateAndIntervalCommand = new Command(SaveDateAndInterval);
            CancelCommand = new Command(CancelButtonClicked);
            dateAndInterval = new DateAndInterval(1, minimumDate, minimumDate);
            this.parentViewModel = parentViewModel;
        }

        // Called when sensor is updated
        public DateAndIntervalViewModel(Sensor sensor, bool updateSensor, SelectSensorsViewModel parentViewModel)
        {
            this.updateSensor = updateSensor;
            this.sensor = sensor;
            SaveDateAndIntervalCommand = new Command(SaveDateAndInterval);
            CancelCommand = new Command(CancelButtonClicked);
            dateAndInterval = sensor.DateAndInterval;
            this.parentViewModel = parentViewModel;
        }

        // Called when all sensors is activated
        public DateAndIntervalViewModel(ObservableCollection<Sensor> sensorList, SelectSensorsViewModel parentViewModel)
        {
            dateAndInterval = new DateAndInterval(1, minimumDate, minimumDate);
            activateAllSensors = true;
            this.sensorList = sensorList;
            this.parentViewModel = parentViewModel;
            SaveDateAndIntervalCommand = new Command(SaveDateAndInterval);
            CancelCommand = new Command(CancelButtonClicked);
        }

        public DateAndInterval DateAndInterval
        {
            set { SetProperty(ref dateAndInterval, value); }
            get { return dateAndInterval; }
        }

        public DateTime MinimumDate
        {
            set { SetProperty(ref minimumDate, value); }
            get { return minimumDate; }
        }

        public Command SaveDateAndIntervalCommand { get; }
        public Command CancelCommand { get; }

        private async void SaveDateAndInterval(object obj)
        {
            bool addSensorToDb = false;

            if (activateAllSensors)
            {
                List<Sensor> sensorsInDb = (List<Sensor>)await App.DatabaseContext.GetSensorsAsync();

                if(sensorsInDb.Count > 0)
                {
                    foreach (Sensor sensorInList in sensorList)
                    {
                        sensorInList.DateAndInterval = new DateAndInterval(
                        DateAndInterval.Interval,
                        DateAndInterval.FromDate,
                        DateAndInterval.ToDate);

                        foreach (Sensor sensorInDb in sensorsInDb)
                        {
                            if(sensorInList.Uuid == sensorInDb.Uuid)
                            {
                                await App.DatabaseContext.UpdateSensorAsync(sensorInDb);
                                addSensorToDb = false;
                                break;
                            }
                            addSensorToDb = true;
                        }
                        if (addSensorToDb)
                        {
                            await App.DatabaseContext.AddSensorAsync(sensorInList);
                        }
                    }
                }
                else
                {
                    foreach (Sensor sensorInList in sensorList)
                    {
                        sensorInList.DateAndInterval = new DateAndInterval(
                                                DateAndInterval.Interval,
                                                DateAndInterval.FromDate,
                                                DateAndInterval.ToDate);

                        await App.DatabaseContext.AddSensorAsync(sensorInList);
                    }
                }

                await PopupNavigation.Instance.PopAllAsync();

                var toastMessage = new ToastMessage("All sensors activated");
                toastMessage.ShowToast();
            }

            else
            {
                sensor.DateAndInterval = dateAndInterval;

                if (updateSensor)
                {
                    await App.DatabaseContext.UpdateSensorAsync(sensor);
                    await PopupNavigation.Instance.PopAllAsync();

                    var toastMessage = new ToastMessage(sensor.Name + " updated");
                    toastMessage.ShowToast();
                }
                else
                {
                    await App.DatabaseContext.AddSensorAsync(sensor);
                    await PopupNavigation.Instance.PopAllAsync();

                    var toastMessage = new ToastMessage(sensor.Name + " activated");
                    toastMessage.ShowToast();
                }
            }
        }

        private async void CancelButtonClicked(object obj)
        {
            ObservableCollection<Sensor> parentSensorList = parentViewModel.SensorList;

            foreach(Sensor sensorInList in parentSensorList)
            { 
                if(sensor.Id == sensorInList.Id)
                    sensorInList.IsActive = false;
            }
            await PopupNavigation.Instance.PopAllAsync();
        }

    }
}
