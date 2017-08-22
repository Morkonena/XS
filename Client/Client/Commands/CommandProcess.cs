using System;
using Android.App;

using XC.Activities;
using XC.Utilities;

namespace XC.Commands
{
    class CommandProcess
    {
        public ConsoleActivity Console;

        public string Command;
        public string Name;

        public Uid Id;

        public CommandProcess (string command, string name, Uid id, bool open, RootActivity root)
        {
            Console = new ConsoleActivity(root, id, open);
            Command = command;
            Name = name;
            Id = id;
        }

        public void Show ()
        {
            Console.Show();
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
            Connection.Send(MessageType.Stop, new StopRequest(Id));
            CommandActivity.Running.Remove(this);        
        }
    }
}