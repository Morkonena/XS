using Android.App;
using Android.Preferences;
using Android.Widget;

using System.Net;

using XC.Login;
using XC.Utilities;
using System;
using Android.Content;

namespace XC.Activities
{
    class LoginActivity : Alias
    {
        public static void Initialize (RootActivity root, Action finished)
        {
            var prefrences = root.GetPreferences(FileCreationMode.Private);
            
            if (string.IsNullOrEmpty(prefrences.GetString("Address", null)))
            {
                Setup(root, new string[] { string.Empty, string.Empty, string.Empty }, prefrences.Edit(), finished);
            }
            else
            {
                Connection.ConnectionInfo = new IPEndPoint(IPAddress.Parse(prefrences.GetString("Address", null)), prefrences.GetInt("Port", 0));
                Cryptography.Initialize(prefrences.GetString("Key", null));

                finished();
            }
        }

        public static void Setup (RootActivity root, string[] defaults, ISharedPreferencesEditor editor, Action finished)
        {
            root.SetContentView(Resource.Layout.Empty);

            var builder = new AlertDialog.Builder(root);
            builder.SetTitle("Yhteys");
            builder.SetView(Resource.Layout.Setup);
            builder.SetCancelable(false);

            builder.SetPositiveButton("OK", (sender, arguments) =>
            {
                try
                {
                    var address = ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Setup_Address).Text;
                    var port = ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Setup_Port).Text;
                    var key = ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Setup_Key).Text;

                    IPAddress ip;

                    if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(key))
                    {
                        root.ShowToast("Kaikki kentät täytyy olla täytettyinä");
                        Setup(root, defaults, editor, finished);
                        return;
                    }
                    else if (!IPAddress.TryParse(address, out ip))
                    {
                        root.ShowToast("Osoite oli virheellinen!");
                        Setup(root, defaults, editor, finished);
                        return;
                    }
                    else if ((key = key.Replace("-", "")).Length != 16)
                    {
                        root.ShowToast("Avain täytyy olla 16 merkkiä pitkä!");
                        Setup(root, defaults, editor, finished);
                        return;
                    }

                    Connection.ConnectionInfo = new IPEndPoint(ip, int.Parse(port));
                    Cryptography.Initialize(key);

                    editor.PutString("Address", address);
                    editor.PutInt("Port", Connection.ConnectionInfo.Port);
                    editor.PutString("Key", key); // Encryption?
                    editor.Commit();                      
                }
                catch (Exception e)
                {
                    root.ShowDialog(e.ToString(), "Virhe");
                    return;
                }

                finished();
            });

            var dialog = builder.Show();

            dialog.FindViewById<EditText>(Resource.Id.Setup_Address).Text = defaults[0];
            dialog.FindViewById<EditText>(Resource.Id.Setup_Port).Text = defaults[1];
            dialog.FindViewById<EditText>(Resource.Id.Setup_Key).Text = defaults[2];
        }

        public LoginActivity (RootActivity root) : base(root)
        {
            Connection.Callback = (type, bytes) =>
            {
                switch (type)
                {
                    case MessageType.Succeeded:
                    {
                        RunOnUiThread(() => { new CommandActivity(bytes, GetRoot()); });
                        break;
                    }
                    case MessageType.Failed:
                    {
                        RunOnUiThread(() => { OpenDialog(); });
                        ShowToast("Kirjautuminen epäonnistui!");
                        break;
                    }
                    default:
                    {
                        ShowToast("Palvelimella tapahtui virhe!");
                        break;
                    }
                }
            };

            OpenDialog();
        }

        public void OpenDialog ()
        {
            SetContentView(Resource.Layout.Empty);

            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Kirjaudu");
            builder.SetCancelable(false);
            builder.SetView(Resource.Layout.Login);

            builder.SetPositiveButton("OK", (dialog, arguments) =>
            {
                var text = ((AlertDialog)dialog).FindViewById<EditText>(Resource.Id.Login_Password).Text;

                if (string.IsNullOrEmpty(text))
                {
                    ShowToast("Salasana kenttä ei voi olla tyhjä!");
                    OpenDialog();
                    return;    
                }

                SetContentView(Resource.Layout.Wait);
                Connection.Send(MessageType.Login, new LoginRequest(text));
            });

            builder.SetNegativeButton("Asetukset", (sender, arguments) =>
            {
                Connection.Shutdown();
                Connection.Disconnect(true);

                var prefrences = GetRoot().GetPreferences(FileCreationMode.Private);
                var port = prefrences.GetInt("Port", -1);

                LoginActivity.Setup(GetRoot(), new string[] { prefrences.GetString("Address", string.Empty), (port == -1 ? string.Empty : port.ToString()), prefrences.GetString("Key", string.Empty) }, prefrences.Edit(), () =>
                {
                    Connection.Initialize(GetRoot());
                });
            });

            builder.Show();
        }
    }
}