using System;
using Android.App;
using Android.Support.V7.App;

namespace SensorGateway.Droid
{
    [Activity(Label = "Sensor Gateway App", Icon = "@mipmap/icon", Theme = "@style/splashscreen", MainLauncher = false, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(typeof(MainActivity));
        }
    }
}
