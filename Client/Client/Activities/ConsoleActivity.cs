using Android.App;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading;

using XC.Commands;
using XC.Utilities;

namespace XC.Activities
{
    class ConsoleActivity : Alias
    {
        private Queue<string> Input = new Queue<string>();
        private Queue<string> Output = new Queue<string>();

        private Thread Worker;

        private bool Open = true;

        private TextView Text;
        private EditText Field;

        private Uid Id;
       
        public ConsoleActivity (RootActivity root, Uid id, bool open) : base(root)
        {
            Id = id;

            if (open)
            {
                Show();
            }
        }

        private string Combine (Queue<string> queue, int count)
        {
            string result = string.Empty;

            for (int i = 0; i < count; i++)
            {
                result += queue.Dequeue() + "\n";
            }

            return result;
        }

        private void Queue ()
        {
            while (Open)
            {
                Thread.Sleep(16);

                var count = Input.Count;
                
                if (count > 0)
                {
                    var text = Combine(Input, count);

                    RunOnUiThread(() => { Text.Text += text; });
                }

                if ((count = Output.Count) > 0)
                {
                    var text = Combine(Output, count);

                    RunOnUiThread(() => { Text.Text += text; });
                }            
            }
        }

        public void Append (string text)
        {
            Output.Enqueue(text);
        }

        public void Show ()
        {
            SetContentView(Resource.Layout.Console);

            Text = Find<TextView>(Resource.Id.Console_Text);
            Field = Find<EditText>(Resource.Id.Console_Input);

            Field.TextChanged += (sender, arguments) =>
            {
                if (arguments.AfterCount > 0 && Field.Text[arguments.Start] == '\n')
                {
                    var input = Field.Text.Substring(0, Field.Text.Length - 1);
                    
                    Connection.Send(MessageType.Console, new ConsoleMessage(Id, input));

                    Input.Enqueue("> " + input);
                    Field.Text = string.Empty;
                }
            };

            Open = true;

            Worker = new Thread(Queue);
            Worker.Start();
        }

        public void Close ()
        {
            Open = false;

            try
            {
                Worker.Join();
            }
            catch {}

            Input.Clear();
            Output.Clear();            
        }
    }
}