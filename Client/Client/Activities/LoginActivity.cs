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
        public static void Initialize (RootActivity root, Action onFinished)
        {
            var prefrences = root.GetPreferences(FileCreationMode.Private);
            
            if (string.IsNullOrEmpty(prefrences.GetString("Address", null)))
            {
                Setup(root, prefrences.Edit(), onFinished);
            }
            else
            {
                Connection.ConnectionInfo = new IPEndPoint(IPAddress.Parse(prefrences.GetString("Address", null)), prefrences.GetInt("Port", 0));
                Cryptography.Initialize(prefrences.GetString("Key", null));

                onFinished();
            }
        }

        public static void Setup (RootActivity root, ISharedPreferencesEditor editor, Action onFinished)
        {
            var builder = new AlertDialog.Builder(root);
            builder.SetTitle("Palvelin");
            builder.SetView(Resource.Layout.Setup);
            builder.SetCancelable(false);

            builder.SetPositiveButton("OK", (sender, arguments) =>
            {
                try
                {
                    var values = new string[] // Better this way
                    {
                        ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Setup_Address).Text,
                        ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Setup_Port).Text,
                        ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Setup_Key).Text
                    };

                    IPAddress address;
                    int port;

                    if (string.IsNullOrEmpty(values[0]) || string.IsNullOrEmpty(values[1]) || string.IsNullOrEmpty(values[2]))
                    {
                        root.ShowToast("Kaikki kentät täytyy olla täytettyinä");
                        return;
                    }
                    else if (!IPAddress.TryParse(values[0], out address))
                    {
                        root.ShowToast("Osoite oli virheellinen!");
                        return;
                    }
                    else if (!int.TryParse(values[1], out port)) // Unnecessary...
                    {
                        root.ShowToast("Portti oli virheellinen!");
                        return;
                    }
                    else if ((values[2] = values[2].Replace("-", "")).Length != 32)
                    {
                        root.ShowToast("Avain täytyy olla 32 merkkiä pitkä!");
                        return;
                    }

                    editor.PutString("Address", values[0]);
                    editor.PutInt("Port", port);
                    editor.PutString("Key", values[2]); // Encryption?
                    editor.Commit();

                    Connection.ConnectionInfo = new IPEndPoint(address, port);
                    Cryptography.Initialize(values[2]);               
                }
                catch (Exception e)
                {
                    root.ShowDialog(e.ToString(), "Virhe");
                    return;
                }

                onFinished();
            });

            builder.Show();
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
            builder.SetTitle("Kirjautuminen");
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

            builder.SetNegativeButton("Muokkaa", (sender, arguments) =>
            {

            });

            builder.Show();
        }
    }
}