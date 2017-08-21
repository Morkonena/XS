using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.Views;

using Client.Commands;
using Client.Utilities;
using Client.Login;

namespace Client.Activities
{
    class CommandActivity : Alias
    {
        public static List<CommandInfo> Commands = new List<CommandInfo>();
        public static List<CommandProcess> Running = new List<CommandProcess>();

        public static CommandProcess GetProcess (Uid id)
        {
            return Running.Find(process => process.Id == id);
        }

        private ListView CommandList;

        public CommandActivity (Activity activity) : base(activity)
        {
            // Alusta komento aktiviteetti
            Initialize();
        }

        public CommandActivity (byte[] bytes, Activity activity) : base(activity)
        {
            try
            {
                Commands = Utility.Convert<LoginRespond>(bytes).Commands.ToList();
            }
            catch (Exception e)
            {
                ShowDialog("Komentopaketin purkaminen epäonnistui: " + e.ToString(), "Virhe");
                return;
            }

            // Alusta komento aktiviteetti
            Initialize();
        }

        private void Initialize ()
        {
            SetContentView(Resource.Layout.Commands); // Määritä pääikkunan sisältö

            Find(Resource.Id.CommandAdd).Click += OnProjectManagement;

            // Määritä komentolistan toiminnot
            CommandList = (ListView)Find(Resource.Id.CommandList);      
            CommandList.ItemClick += (Sender, Arguments) => { OnCommandSelected(Commands[Arguments.Position]); };
            CommandList.ItemLongClick += (Sender, Arguments) => { OnDeleteCommand(Commands[Arguments.Position].Name); };

            // Päivitä komentolista
            Refresh();
        }

        private void Refresh()
        {
            // Tarkista, että suoritettavia komentoja on olemassa
            if (Commands.Count == 0)
            {
                Find(Resource.Id.CommandsNone).Visibility = ViewStates.Visible; // Näytä 'Yhtään komentoa ei löytynyt' teksti
                CommandList.Adapter = null; // Poista listalta valinnat
            }
            else
            {
                CommandList.Adapter = new ArrayAdapter(GetActivity(), Android.Resource.Layout.SimpleListItem1, Commands.Select(Command => Command.Name).ToArray()); // Aseta listalle valinnat
            }
        }

        private void OnProjectManagement(object a, EventArgs b)
        {
            var Builder = new AlertDialog.Builder(GetActivity());

            // Laita ikkunan vaihtoehdoiksi 'Luo', 'Avaa' ja 'Poista'
            Builder.SetAdapter(new ArrayAdapter(GetActivity(), Android.Resource.Layout.SimpleListItem1, new string[] { "Luo", "Avaa", "Poista" }), (Sender, Arguments) =>
            {
                switch (Arguments.Which)
                {
                    case 0:
                    {
                        OnCreateProject(); // Luo uusi projekti
                        break;
                    }
                    case 1:
                    {
                        OnOpenProject(); // Avaa projekti
                        break;
                    }
                    case 2:
                    {
                        OnDeleteProject(); // Poista projekti
                        break;
                    }                    
                }
            });

            Builder.Show();
        }

        private void OnCreateProject ()
        {
            // Anna käyttäjän valita minne kansioon projekti luodaan
            Selection.SelectFolder(GetActivity(), Selection.InternalStorage, (string Folder) =>
            {            
                if (Folder != null)
                {
                    OnProjectName(Folder);
                }
            });
        }

        private void OnProjectName (string Folder)
        {
            var builder = new AlertDialog.Builder(GetActivity());
            builder.SetTitle("Luo");
            builder.SetView(Resource.Layout.CreateProject);

            builder.SetPositiveButton("OK", (Sender, Arguments) =>
            {
                var name = ((EditText)((AlertDialog)Sender).FindViewById(Resource.Id.CreateProject_Name)).Text;

                // Tarkista, ettei nimi ole tyhjä
                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Nimi kenttä ei voi olla tyhjä!");
                    return;
                }

                try
                {
                    Directory.CreateDirectory(Folder + "/" + name); // Luo projekti kansio
                }
                catch (Exception e)
                {
                    ShowDialog("Projektin luominen epäonnistui: " + e.ToString(), "Virhe");
                    return;
                }

                new EditorActivity(GetActivity(), Folder + "/" + name); // Avaa editori äskettäin luodusta kansiosta
            });

            builder.SetNegativeButton("Peruuta", (Sender, Arguments) => {}); 
            builder.Show(); 
        }

        private void OnOpenProject ()
        {
            // Valitse kansio missä projekti sijaitsee
            Selection.SelectFolder(GetActivity(), Selection.InternalStorage, (string Path) =>
            {
                // Tarkista, ettei käyttäjä peruuttanut
                if (Path != null)
                {
                    new EditorActivity(GetActivity(), Path);
                }
            });
        }

        private void OnDeleteProject ()
        {
            // Anna käyttäjän valita kansio missä projekti sijaitsee
            Selection.SelectFolder(GetActivity(), Selection.InternalStorage, (string Path) =>
            {
                // Tarkista, ettei käyttäjä peruuttanut;
                if (Path != null)
                {
                    try
                    {
                        Directory.Delete(Path, true); // Poista valittu kansio
                    }
                    catch (Exception e)
                    {
                        ShowDialog("Projekti kansion poistaminen epäonnistui: " + e.ToString(), "Virhe");
                    }
                }       
            });
        }

        private void OnDeleteCommand (string Name)
        {
            AlertDialog.Builder Builder = new AlertDialog.Builder(GetActivity());
            Builder.SetTitle("Poista"); 
            Builder.SetMessage("Haluatko varmasti poistaa tämän komennon?");

            Builder.SetPositiveButton("Kyllä", (Sender, Arguments) =>
            {
                // Poista komento listasta ja lopeta kaikki prosessit, jotka käyttävät poistettavaa komentoa
                Commands.RemoveAll(command => command.Name == Name);
                Running.RemoveAll(Process => Process.Command == Name);
               
                // Päivitä komentolista
                Refresh();

                // Pyydä palvelinta poistamaan komento
                Connection.Send(MessageType.Delete, new DeleteRequest(Name));
            }); 

            Builder.SetNegativeButton("Ei", (Sender, Arguments) => { });
            Builder.Show(); 
        }

        private void OnCommandSelected (CommandInfo command)
        {
            var builder = new AlertDialog.Builder(GetActivity());
            builder.SetTitle(command.Name);
            builder.SetView(Resource.Layout.ExecuteLayout);

            builder.SetPositiveButton("Aloita", (Sender, Arguments) =>
            {
                var name = ((EditText)((AlertDialog)Sender).FindViewById(Resource.Id.ProcessName)).Text;
                var open = ((CheckBox)((AlertDialog)Sender).FindViewById(Resource.Id.ConsoleCheckbox)).Checked;

                // Tarkista, ettei nimi ole tyhjä
                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Nimi kenttä ei voi olla tyhjä!");
                    return;
                }

                OnStart(command, name, open);
            });

            var dialog = builder.Show();
            var processes = Running.Where(Process => (Process.Command == command.Name)).ToArray(); // Etsi kaikki prosessit, jotka käyttävät valittua komentoa

            // Tarkista, että prosesseja löytyi
            if (processes.Length > 0)
            {
                ((TextView)dialog.FindViewById(Resource.Id.NoProcesses)).Visibility = ViewStates.Invisible; // Piilota 'Ei prosesseja' teksti

                var list = (ListView)dialog.FindViewById(Resource.Id.ProcessList);
                list.Adapter = new ArrayAdapter(GetActivity(), Android.Resource.Layout.SimpleListItem1, processes.Select(Process => Process.Name).ToArray()); // Määritä prosessilistan valinnat

                // Määritä prosessin avaaminen
                list.ItemClick += (sender, arguments) =>
                {
                    processes[arguments.Position].Open();
                };

                // Määritä prosessin lopettaminen 
                list.ItemLongClick += (sender, arguments) =>
                {
                    OnStopProcess(processes[arguments.Position], dialog);
                };
            }

            if (command.Description.Length > 0)
            {
                ((TextView)dialog.FindViewById(Resource.Id.CommandDescription)).Text = command.Description;
            }       
        }

        private void OnStopProcess (CommandProcess Process, AlertDialog Dialog)
        {
            var builder = new AlertDialog.Builder(GetActivity());
            builder.SetTitle("Sulje");
            builder.SetMessage("Haluatko varmasti sulkea tämän prosessin?");

            builder.SetPositiveButton("Kyllä", (Sender, Arguments) =>
            {
                Process.OnExit(); // Lopeta prosessi
                Dialog.Dismiss(); // Sulje komentoikkuna
            });

            builder.SetNegativeButton("Ei", (Sender, Arguments) => {});
            builder.Show();
        }

        private void OnStart (CommandInfo command, string name, bool open)
        {
            Connection.Callback = (type, data) =>
            {
                switch (type)
                {
                    case MessageType.Succeeded:
                    {
                        // Lisää prosesseihin uusi prosessi
                        Running.Add(new CommandProcess(command.Name, name, Utility.Convert<StartRespond>(data).Id, open, GetActivity()));
                        break;
                    }
                    case MessageType.Failed:
                    {
                        ShowToast("Komennon aloittaminen epäonnistui!");
                        break;
                    }
                    default:
                    {
                        ShowToast("Palvelimella tapahtui virhe!");
                        break;
                    }
                }
            };

            // Pyydä palvelinta aloittamaan prosessi
            Connection.Send(MessageType.Start, new StartRequest(command.Name));
        }
    }
}