using System;
using Acr.UserDialogs;

namespace SensorGateway.Toast
{
    public class ToastMessage
    {
        readonly string toastMessage;

        public ToastMessage(string toastMessage)
        {
            this.toastMessage = toastMessage;
        }

        public void ShowToast()
        {
            var toastConfig = new ToastConfig(toastMessage);
            toastConfig.SetDuration(3000);
            toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(44, 181, 19));
            toastConfig.SetMessageTextColor(System.Drawing.Color.FromArgb(0, 0, 0));
            UserDialogs.Instance.Toast(toastConfig);
        }
    }
}
