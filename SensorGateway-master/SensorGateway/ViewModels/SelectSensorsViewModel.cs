using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Rg.Plugins.Popup.Services;
using Plugin.BluetoothLE;
using SensorGateway.Models;
using SensorGateway.Services;
using SensorGateway.Toast;
using SensorGateway.Views;
using Xamarin.Forms;
using System;

namespace SensorGateway.ViewModels
{
    public class SelectSensorsViewModel : ViewModelBase
    {
        private ObservableCollection<Sensor> sensorList;
        private bool allSelected;
        private bool propertyChangeAdded;
        private DateAndIntervalViewModel childViewModel;
        private DateTime minimumDate = DateTime.Today;
        private SensorServices sensorService;
        private bool isBusy;

        public SelectSensorsViewModel()
        {
            IsBusy = true;
            sensorList = new ObservableCollection<Sensor>();
            sensorService = new SensorServices(this);

            //MessagingCenter.Send(this, "StopTSensorConnection");
            //MessagingCenter.Send(this, "StartTSensorConnection");

            //sensorList = sensorService.GetSensors(); // mockup data todo remove later

            sensorService.RefreshSensors(); // todo måste köras på egen tråd för att inte crascha på iOS

            SensorList.CollectionChanged += SensorList_CollectionChanged;
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                SetProperty(ref isBusy, value);
            }
        }

        public INavigation Navigation { get; set; }

        public ObservableCollection<Sensor> SensorList
        {
            get { return sensorList; }
            set { SetProperty(ref sensorList, value); }
        }

        public Command ClickedSelectAllSensorsCommand 
        {
            get
            {
                return new Command(async () =>
                {
                    RemovePropertyChanged();

                    if (!allSelected)
                    {
                        foreach (Sensor sensor in sensorList)
                            sensor.IsActive = true;

                        childViewModel = new DateAndIntervalViewModel(sensorList, this);

                        await PopupNavigation.Instance.PushAsync(new DateAndIntervalPopup()
                        {
                            BindingContext = childViewModel
                        });

                        allSelected = true;
                    }

                    else
                    {
                        List<Sensor> sensorsInDb = (List<Sensor>)await App.DatabaseContext.GetSensorsAsync();

                        foreach (Sensor sensor in sensorList)
                        {
                            if(sensor.IsActive)
                                sensor.IsActive = false;
                        }

                        foreach(Sensor sensorInDb in sensorsInDb)
                            await App.DatabaseContext.DeleteSensorAsync(sensorInDb.Id);

                        allSelected = false;

                        var toastMessage = new ToastMessage("All sensors deactivated");
                        toastMessage.ShowToast();

                    }

                    AddPropertyChanged();
                });
            }
        }

        public Command<Sensor> UpdateSensorCommand
        { 
            get
            {
                return new Command<Sensor>(async (sensor) =>
                {
                    bool isSensorInDb = true;
                    childViewModel = new DateAndIntervalViewModel(sensor, isSensorInDb, this);

                    await PopupNavigation.Instance.PushAsync(new DateAndIntervalPopup()
                    {
                        BindingContext = childViewModel
                    });
                });
            }
        }

        private void SensorList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                IsBusy = false;

                foreach (Sensor sensor in e.NewItems)
                    sensor.PropertyChanged += Sensor_PropertyChanged;
            }
            if (e.OldItems != null)
                foreach (Sensor sensor in e.OldItems)
                    sensor.PropertyChanged -= Sensor_PropertyChanged;
        }

        async void Sensor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            List<Sensor> sensorsInDb = (List<Sensor>)await App.DatabaseContext.GetSensorsAsync();

            if (sender is Sensor sensor)
            {
                if (!sensor.IsActive)
                {
                    allSelected = false;

                    foreach (Sensor sensorDb in sensorsInDb)
                    {
                        if (sensor.Uuid == sensorDb.Uuid)
                        {
                            Sensor sensorToRemove = sensorDb;
                            await App.DatabaseContext.DeleteSensorAsync(sensorToRemove.Id);
                        }
                    }

                    var toastMessage = new ToastMessage(sensor.Name + " deactivated");
                    toastMessage.ShowToast();

                }
                else
                {
                    childViewModel = new DateAndIntervalViewModel(sensor, this);

                    if (sensor.IsActive)
                    {
                        await PopupNavigation.Instance.PushAsync(new DateAndIntervalPopup()
                        {
                            BindingContext = childViewModel
                        });
                    }
                }
            }
        }

        // Compares the Sensors in range with the Sensors in database
        public void CheckActiveSensors(List<Sensor> sensorsInDb)
        {
            int sensorActiveCounter = 0;

            if (sensorsInDb != null)
            {
                foreach (Sensor sensorDb in sensorsInDb)
                {
                    foreach (Sensor sensor in sensorList)
                    {
                        if (sensorDb.Uuid == sensor.Uuid)
                        {
                            sensor.DateAndInterval = sensorDb.DateAndInterval;
                            sensor.IsActive = true;
                            sensorActiveCounter++;
                        }
                    }
                }
            }
            allSelected |= sensorActiveCounter == sensorList.Count;

            if (!propertyChangeAdded)
            {
                AddPropertyChanged();
                propertyChangeAdded = true;
            }
        }

        private void AddPropertyChanged()
        {
            foreach (Sensor sensor in sensorList)
                sensor.PropertyChanged += Sensor_PropertyChanged;
        }

        private void RemovePropertyChanged()
        {
            foreach (Sensor sensor in sensorList)
                sensor.PropertyChanged -= Sensor_PropertyChanged;
        }
    }

    public class StartBluetoothTask
    {
        public IDevice device;

        public StartBluetoothTask(IDevice device)
        {
            this.device = device;

            MessagingCenter.Send(this, "BluetoothConnection");
        }
    }
}

