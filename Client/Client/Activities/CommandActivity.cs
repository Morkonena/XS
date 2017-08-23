using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.Views;

using XC.Commands;
using XC.Utilities;
using XC.Login;

namespace XC.Activities
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

        public CommandActivity (RootActivity root) : base(root)
        {
            Initialize();
        }

        public CommandActivity (byte[] bytes, RootActivity root) : base(root)
        {
            try
            {
                Commands = Utility.Convert<LoginRespond>(bytes).Commands.ToList();
            }
            catch (Exception e)
            {
                ShowError(e);
                return;
            }

            Initialize();
        }

        private void Initialize ()
        {
            SetContentView(Resource.Layout.Commands);

            Find<ImageView>(Resource.Id.Commands_Add).Click += OnProjectManagement;

            CommandList = Find<ListView>(Resource.Id.Commands_List);      

            CommandList.ItemClick += (Sender, Arguments) => OnCommandSelected(Commands[Arguments.Position]);
            CommandList.ItemLongClick += (Sender, Arguments) => OnDeleteCommand(Commands[Arguments.Position].Name);

            Refresh();
        }

        private void Refresh()
        {
            if (Commands.Count == 0)
            {
                Find<TextView>(Resource.Id.Commands_None).Visibility = ViewStates.Visible; // Näytä 'Yhtään komentoa ei löytynyt' teksti
                CommandList.Adapter = null; 
            }
            else
            {
                CommandList.Adapter = new ArrayAdapter(GetContext(), Android.Resource.Layout.SimpleListItem1, Commands.Select(Command => Command.Name).ToArray()); // Aseta listalle valinnat
            }
        }

        private void OnProjectManagement(object a, EventArgs b)
        {
            var builder = new AlertDialog.Builder(GetContext());

            builder.SetAdapter(new ArrayAdapter(GetContext(), Android.Resource.Layout.SimpleListItem1, new string[] { "Luo", "Avaa", "Poista" }), (sender, arguments) =>
            {
                switch (arguments.Which)
                {
                    case 0:
                    {
                        OnCreateProject();
                        break;
                    }
                    case 1:
                    {
                        OnOpenProject();
                        break;
                    }
                    case 2:
                    {
                        OnDeleteProject();
                        break;
                    }                    
                }
            });

            builder.Show();
        }

        private void OnCreateProject ()
        {
            Selection.SelectFolder(GetContext(), Selection.InternalStorage, (string Folder) =>
            {            
                if (Folder != null)
                {
                    OnProjectName(Folder);
                }
            });
        }

        private void OnProjectName (string Folder)
        {
            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Luo");
            builder.SetView(Resource.Layout.Project);

            builder.SetPositiveButton("OK", (Sender, Arguments) =>
            {
                var name = ((EditText)((AlertDialog)Sender).FindViewById(Resource.Id.Project_Name)).Text;

                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Nimi ei voi olla tyhjä!");
                    return;
                }

                try
                {
                    Directory.CreateDirectory(Folder + "/" + name); 
                }
                catch (Exception e)
                {
                    ShowDialog("Projektin luominen epäonnistui: " + e.ToString(), "Virhe");
                    return;
                }

                new EditorActivity(GetRoot(), Folder + "/" + name); 
            });

            builder.SetNegativeButton("Peruuta", (Sender, Arguments) => {}); 
            builder.Show(); 
        }

        private void OnOpenProject ()
        {
            Selection.SelectFolder(GetContext(), Selection.InternalStorage, (path) =>
            {
                if (path != null)
                {
                    new EditorActivity(GetRoot(), path);
                }
            });
        }

        private void OnDeleteProject ()
        {
            Selection.SelectFolder(GetContext(), Selection.InternalStorage, (path) =>
            {
                if (path != null)
                {
                    try
                    {
                        Directory.Delete(path, true); 
                    }
                    catch (Exception e)
                    {
                        ShowError(e);
                    }
                }       
            });
        }

        private void OnDeleteCommand (string Name)
        {
            AlertDialog.Builder Builder = new AlertDialog.Builder(GetContext());
            Builder.SetTitle("Poista"); 
            Builder.SetMessage("Haluatko varmasti poistaa tämän komennon?");

            Builder.SetPositiveButton("Kyllä", (Sender, Arguments) =>
            {
                Commands.RemoveAll(command => command.Name == Name);
                Running.RemoveAll(Process => Process.Command == Name);
               
                Refresh();

                Connection.Send(MessageType.Delete, new DeleteRequest(Name));
            }); 

            Builder.SetNegativeButton("Ei", (Sender, Arguments) => { });
            Builder.Show(); 
        }

        private void OnCommandSelected (CommandInfo command)
        {
            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle(command.Name);
            builder.SetView(Resource.Layout.Execute);

            builder.SetPositiveButton("Aloita", (sender, arguments) =>
            {
                var name = ((AlertDialog)sender).FindViewById<EditText>(Resource.Id.Execute_Name).Text;
                var open = ((AlertDialog)sender).FindViewById<CheckBox>(Resource.Id.Execute_Console).Checked;

                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Nimi ei voi olla tyhjä!");
                    return;
                }

                OnStart(command, name, open);
            });

            builder.SetNegativeButton("Peruuta", (sender, arguments) => { });

            var dialog = builder.Show();
            var processes = Running.Where(Process => (Process.Command == command.Name)).ToArray(); 

            if (processes.Length > 0)
            {
                var list = dialog.FindViewById<ListView>(Resource.Id.Execute_List);

                list.Adapter = new ArrayAdapter(GetContext(), Android.Resource.Layout.SimpleListItem1, processes.Select(process => process.Name).ToArray()); 
                list.ItemClick += (sender, arguments) => processes[arguments.Position].Show();
                list.ItemLongClick += (sender, arguments) => OnStopProcess(processes[arguments.Position], dialog);
            }

            if (command.Description.Length > 0)
            {
                dialog.FindViewById<TextView>(Resource.Id.Execute_Description).Text = command.Description;
            }       
        }

        private void OnStopProcess (CommandProcess process, AlertDialog dialog)
        {
            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Sulje");
            builder.SetMessage("Haluatko varmasti sulkea tämän prosessin?");

            builder.SetPositiveButton("Kyllä", (sender, arguments) =>
            {
                process.OnExit(); 
                dialog.Dismiss();
            });

            builder.SetNegativeButton("Ei", (sender, arguments) => {});
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
                        Running.Add(new CommandProcess(command.Name, name, Utility.Convert<StartRespond>(data).Id, open, GetRoot()));
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

            Connection.Send(MessageType.Start, new StartRequest(command.Name));
        }
    }
}