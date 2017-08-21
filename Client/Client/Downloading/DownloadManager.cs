using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Threading.Tasks;

using Client.Activities;
using Client.Utilities;
using Android.Widget;

namespace Client.Uploading
{
    class DownloadManager
    {
        public static List<Task> Downloads { get; set; } = new List<Task>();

        public const int Timeout = 5000;

        private static byte[] Receive(Socket socket, int count)
        {
            while (socket.Available < count) { }

            var buffer = new byte[count];
            socket.Receive(buffer);

            return buffer;
        }

        public static void StartAsync(Base application, int port, int bufferSize, string filename)
        {
            Downloads.Add(Task.Run(() => { Start(application, port, bufferSize, filename); }));
        }

        private static void Start(Base application, int port, int bufferSize, string filename)
        {
            try
            {
                var connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 1000000
                };

                var result = connection.BeginConnect(new IPEndPoint(Connection.ConnectionInfo.Address, port), null, null);

                if (!result.AsyncWaitHandle.WaitOne(Timeout, true))
                {
                    connection.Close();

                    application.RunOnUiThread(() => { Toast.MakeText(application, "Latausyhteyttä ei saatu muodostettua!", ToastLength.Long).Show(); });
                    return;
                }

                using (var stream = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    while (true)
                    {
                        var length = BitConverter.ToInt32(Receive(connection, 4), 0);

                        if (length == -1)
                        {
                            break;
                        }
                        else
                        {
                            stream.Write(Receive(connection, length), 0, length);
                        }
                    }
                }

                connection.Close();
            }
            catch (Exception e)
            {
                application.RunOnUiThread(() => { Toast.MakeText(application, "Latauksessa tapahtui virhe: " + e.ToString(), ToastLength.Long).Show(); });
            }
            finally
            {
                Finish();
            }
        }

        private static void Finish ()
        {
            lock (Downloads)
            {
                for (int i = 0; i < Downloads.Count; i++)
                {
                    if (Downloads[i].Id == Task.CurrentId)
                    {
                        Downloads.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}