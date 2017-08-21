using System;
using Android.App;

using Client.Activities;
using Client.Utilities;

namespace Client.Commands
{
    class CommandProcess
    {
        public ConsoleActivity Console;

        public string Command;
        public string Name;

        public Uid Id;

        public CommandProcess (string command, string name, Uid id, bool open, Activity activity)
        {
            Console = new ConsoleActivity(activity, id, open);
            Command = command;
            Name = name;
            Id = id;
        }

        public void Open ()
        {
            Console.Open();
        }

        public void Append (string text)
        {
            Console.Append(text);
        }

        public void Close ()
        {
            Console.Close();
        }

        public void OnExit()
        {
            // Pyydä palvelinta lopettamaan prosessi
            Connection.Send(MessageType.Stop, new StopRequest(Id));

            // Poista tämä prosessi listalta
            CommandActivity.Running.Remove(this);        
        }
    }
}