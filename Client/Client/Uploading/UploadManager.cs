﻿using System.Collections.Generic;
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
    class UploadManager
    {
        public static List<Task> Uploads { get; set; } = new List<Task>();

        public const int Timeout = 5000;

        private static void SendEncrypted (Socket socket, byte[] buffer)
        {
            buffer = Cryptography.Encrypt(buffer, Connection.Encryption.Key, Connection.Encryption.IV);

            socket.Send(BitConverter.GetBytes(buffer.Length));
            socket.Send(buffer);
        }

        public static void StartAsync(Base application, int port, int bufferSize, string filename)
        {
            Uploads.Add(Task.Run(() => { Start(application, port, bufferSize, filename); }));
        }

        private static void Start (Base application, int port, int bufferSize, string filename)
        {
            try
            {
                var connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = 1000000
                };

                var result = connection.BeginConnect(new IPEndPoint(Connection.ConnectionInfo.Address, port), null, null);

                if (!result.AsyncWaitHandle.WaitOne(Timeout, true))
                {
                    connection.Close();

                    application.RunOnUiThread(() => { Toast.MakeText(application, "Lähetysyhteyttä ei saatu muodostettua!", ToastLength.Long).Show(); });
                    return;
                }

                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var available = stream.Length;
                    var buffer = new byte[Math.Min(bufferSize, available)];

                    while (true)
                    {
                        stream.Read(buffer, 0, buffer.Length);
                        available -= buffer.Length;

                        SendEncrypted(connection, buffer);

                        if (available == 0)
                        {
                            break;
                        }
                        else if (available < bufferSize)
                        {
                            buffer = new byte[available];
                        }
                    }
                }

                connection.Send(BitConverter.GetBytes(-1));
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