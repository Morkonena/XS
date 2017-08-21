using Android.App;
using Android.Widget;

using Client.Login;
using Client.Utilities;

namespace Client.Activities
{
    class LoginActivity : Alias
    {
        public LoginActivity (Activity activity) : base(activity)
        {
            Connection.Callback = (type, bytes) =>
            {
                switch (type)
                {
                    case MessageType.Succeeded:
                    {
                        RunOnUiThread(() => { new CommandActivity(bytes, GetActivity()); });
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

            var builder = new AlertDialog.Builder(GetActivity());
            builder.SetTitle("Kirjautuminen");
            builder.SetCancelable(false);
            builder.SetView(Resource.Layout.Login);

            builder.SetPositiveButton("OK", (Dialog, Arguments) =>
            {
                var text = ((EditText)((AlertDialog)Dialog).FindViewById(Resource.Id.LoginPassword)).Text;

                if (string.IsNullOrEmpty(text))
                {
                    ShowToast("Salasana kenttä ei voi olla tyhjä!");
                    OpenDialog();
                    return;    
                }

                SetContentView(Resource.Layout.Wait);

                if (!Connection.Send(MessageType.Login, new LoginRequest(text)))
                {
                    ShowToast("Kirjautumispyynnön lähettäminen epäonnistui!");
                }
            });

            builder.Show();
        }
    }
}