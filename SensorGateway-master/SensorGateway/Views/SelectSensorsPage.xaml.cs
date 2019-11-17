using System;
using System.Collections.Generic;
using SensorGateway.Models;
using SensorGateway.ViewModels;
using Xamarin.Forms;

namespace SensorGateway.Views
{
    public partial class SelectSensorsPage : ContentPage
    {

        public SelectSensorsPage()
        {
            InitializeComponent();
        }

        private void Update_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var sensor = button?.BindingContext as Sensor;
            var vm = BindingContext as SelectSensorsViewModel;
            vm?.UpdateSensorCommand.Execute(sensor);
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("OnAppearing");
            MessagingCenter.Send(this, "StopBluetoothService");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Console.WriteLine("OnDisappearing");

            MessagingCenter.Send(this, "StopSensorService");
            MessagingCenter.Send(this, "StartBluetoothService");
        }
    }
}
