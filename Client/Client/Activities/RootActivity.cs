using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using XC.Utilities;

using System;

using XC.Login;

namespace XC.Activities
{
    [Activity(Label = "Client", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/ClientTheme")]
    public class RootActivity : Activity
    {
        public const int PingCode = -1;
        public const int DisconnectionCode = -2;

        public static Action Back { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Empty);

            if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Denied)
            {
                Request();
            }
            else
            {
                Initialize();
            }            
        }

        public override void OnRequestPermissionsResult(int code, string[] permissions, Permission[] results)
        {
            foreach (var result in results)
            {
                if (result != Permission.Granted)
                {
                    OnDenied();
                    return;
                }
            }

            Initialize();
        }

        public override void OnBackPressed()
        {
            Back?.Invoke();
        }

        public void ShowDialog (string text, string title, bool cancelable = false)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle(title);
            builder.SetMessage(text);
            builder.SetCancelable(cancelable);

            builder.SetPositiveButton("OK", (sender, arguments) => {});

            RunOnUiThread(() => builder.Show());
        }

        public void ShowToast (string text)
        {
            RunOnUiThread(() => Toast.MakeText(this, text, ToastLength.Long).Show());
        }

        private void Request ()
        {
            RequestPermissions(new string[] { Android.Manifest.Permission.WriteExternalStorage, Android.Manifest.Permission.ReadExternalStorage }, 0);
        }

        private void OnDenied ()
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Oikeudet");
            builder.SetMessage("Sovellus tarvitsee kaikkia äskettäin pyydettyjä oikeuksia toimiakseen!");
            builder.SetCancelable(false);
            builder.SetPositiveButton("Yritä uudelleen", (Sender, Arguments) => { Request(); });
            builder.SetNegativeButton("Poistu", (Sender, Arguments) => { Finish(); });
            builder.Show(); 
        }

        private void OnStartupError (Exception e)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Virhe");
            builder.SetMessage("Käynnistäminen epäonnistui: " + e.ToString());
            builder.SetCancelable(false);
            builder.SetPositiveButton("OK", (Sender, Arguments) => { Finish(); });
            builder.Show();
        }

        private void Initialize ()
        {
            SetContentView(Resource.Layout.Wait);

            try
            {
                EditorActivity.Initialize(this);

                LoginActivity.Initialize(this, () =>
                {
                    Connection.Initialize(this);
                });               
            }
            catch (Exception e)
            {
                OnStartupError(e);
            }           
        }
    }
}

