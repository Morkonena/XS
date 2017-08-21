using Android.App;
using Android.Views;
using Android.Widget;
using System;

namespace Client.Utilities
{
    class Alias
    {
        private Activity Device;

        public Alias (Activity device)
        {
            Device = device;
        }

        public void SetContentView (int id)
        {
            Device.SetContentView(id);
        }

        public View Find (int id)
        {
            return Device.FindViewById(id);
        }

        public Activity GetActivity ()
        {
            return Device;
        }

        public void RunOnUiThread (Action action)
        {
            Device.RunOnUiThread(action);
        }

        public void ShowDialog (string text, string title, bool cancelable = false)
        {
            Device.RunOnUiThread(() =>
            {
                AlertDialog.Builder Builder = new AlertDialog.Builder(GetActivity());
                Builder.SetTitle(title);
                Builder.SetMessage(text);
                Builder.SetCancelable(cancelable);
                Builder.SetPositiveButton("OK", (Sender, Arguments) => { });
                Builder.Show();
            });
        }

        public void ShowToast (string text)
        {
            Device.RunOnUiThread(() =>
            {
                Toast.MakeText(Device, text, ToastLength.Long).Show();
            });
        }
    }
}