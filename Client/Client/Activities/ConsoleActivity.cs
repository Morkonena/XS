using Android.App;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading;

using Client.Commands;
using Client.Utilities;

namespace Client.Activities
{
    class ConsoleActivity : Alias
    {
        private Queue<string> InputQueue = new Queue<string>();
        private Queue<string> OutputQueue = new Queue<string>();

        private Thread QueueThread;

        private bool isOpen = true;

        private TextView Text;
        private EditText Input;

        private Uid Id;
       
        public ConsoleActivity (Activity activity, Uid id, bool open) : base(activity)
        {
            Id = id;

            if (open)
            {
                Open();
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
            while (isOpen)
            {
                Thread.Sleep(16);

                int count = InputQueue.Count;
                
                if (count > 0)
                {
                    string text = Combine(InputQueue, count);

                    RunOnUiThread(() => { Text.Text += text; });
                }

                if ((count = OutputQueue.Count) > 0)
                {
                    string text = Combine(OutputQueue, count);

                    RunOnUiThread(() => { Text.Text += text; });
                }            
            }
        }

        public void Append (string text)
        {
            OutputQueue.Enqueue(text + "\n");
        }

        public void Open ()
        {
            SetContentView(Resource.Layout.Console);

            Text = (TextView)Find(Resource.Id.ConsoleText);
            Input = (EditText)Find(Resource.Id.ConsoleInput);

            Input.TextChanged += (sender, arguments) =>
            {
                if (arguments.AfterCount > 0 && Input.Text[arguments.Start] == '\n')
                {
                    var input = Input.Text.Substring(0, Input.Text.Length - 1);
                    
                    Connection.Send(MessageType.Console, new ConsoleMessage(Id, input));

                    InputQueue.Enqueue("> " + input);
                    Input.Text = string.Empty;
                }
            };

            isOpen = true;

            QueueThread = new Thread(Queue);
            QueueThread.Start();
        }

        public void Close ()
        {
            isOpen = false;

            try
            {
                QueueThread.Join();
            }
            catch { }

            InputQueue.Clear();
            OutputQueue.Clear();            
        }
    }
}