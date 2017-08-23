using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Threading.Tasks;

using XC.Activities;
using XC.Utilities;
using Android.Widget;

namespace XC.Uploading
{
    class UploadManager
    {
        public static List<Task> Uploads { get; set; } = new List<Task>();

        public const int Timeout = 5000;

        private static void Send (Socket socket, byte[] buffer)
        {
            var iv = Cryptography.Generate(16);

            buffer = Cryptography.Encrypt(buffer, Connection.Key, iv);

            socket.Send(BitConverter.GetBytes(buffer.Length));
            socket.Send(iv);
            socket.Send(buffer);
        }

        public static void StartAsync(RootActivity application, int port, int bufferSize, string filename)
        {
            Uploads.Add(Task.Run(() => { Start(application, port, bufferSize, filename); }));
        }

        private static void Start (RootActivity root, int port, int capacity, string filename)
        {
            try
            {
                var connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = capacity + 100
                };

                var result = connection.BeginConnect(new IPEndPoint(Connection.ConnectionInfo.Address, port), null, null);

                if (!result.AsyncWaitHandle.WaitOne(Timeout, true))
                {
                    connection.Close();
                    root.ShowToast("Lähetysyhteyttä ei saatu muodostettua!");
                    return;
                }

                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var buffer = new byte[capacity];
                    var length = 0;

                    while (true)
                    {
                        if ((length = stream.Read(buffer, 0, buffer.Length)) != buffer.Length)
                        {
                            Array.Resize(ref buffer, length);

                            Send(connection, buffer);
                            break;                     
                        }
                        else
                        {
                            Send(connection, buffer);
                        }
                    }
                }

                connection.Send(BitConverter.GetBytes(-1));
                connection.Close();
            }
            catch (Exception e)
            {
                root.ShowDialog(e.ToString(), "Virhe");
            }
            finally
            {
                Finish();
            }
        }

        private static void Finish ()
        {
            lock (Uploads)
            {
                for (int i = 0; i < Uploads.Count; i++)
                {
                    if (Uploads[i].Id == Task.CurrentId)
                    {
                        Uploads.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}