using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Threading.Tasks;
using XSC;

namespace Server
{
    class UploadNetworkStream : XSC.NetworkStream
    {
        private Socket Device;

        public UploadNetworkStream (Socket device)
        {
            Device = device;
        }

        public void Send (byte[] bytes)
        {
            var iv = Cryptography.Generate(16);

            bytes = Cryptography.Encrypt(bytes, Server.Key, iv);

            Device.Send(BitConverter.GetBytes(bytes.Length));
            Device.Send(iv);
            Device.Send(bytes);
        }
    }

    class UploadManager
    {
        public static List<Task> Uploads { get; set; } = new List<Task>();
        public static Queue<int> Available { get; set; } = new Queue<int>();

        public static void Initialize(int start, int range)
        {
            for (var i = 0; i < range; i++)
            {
                Available.Enqueue(start + i);
            }
        }

        public static void StartAsync (int port, int bufferSize, Upload upload)
        {
            Uploads.Add(Task.Run(() => { Start(port, bufferSize, upload); }));
        }

        private static void Start (int port, int capacity, Upload upload)
        {
            try
            {
                var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(1);

                var connection = listener.Accept();
                connection.SendBufferSize = capacity + 100;

                upload.Send(new UploadNetworkStream(connection));

                connection.Send(BitConverter.GetBytes(-1));
                connection.Close();
            }
            catch (Exception e)
            {
                Server.Print("Upload error: " + e.ToString());
            }
            finally
            {
                Finish(port);
            }
        }

        private static void Finish(int port)
        {
            lock (Available)
            {
                Available.Enqueue(port);
            }

            lock (Uploads)
            {
                for (var i = 0; i < Uploads.Count; i++)
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