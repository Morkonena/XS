using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Threading.Tasks;
using XSC;

namespace Server
{
    class DownloadManager
    {
        public static List<Task> Downloads { get; set; } = new List<Task>();
        public static Queue<int> Available { get; set; } = new Queue<int>();

        public static void Initialize(int start, int range)
        {
            for (var i = 0; i < range; i++)
            {
                Available.Enqueue(start + i);
            }
        }

        private static byte[] Receive(Socket socket, int length)
        {
            var buffer = new byte[length];
            while (socket.Available < length) { }

            socket.Receive(buffer);

            return buffer;
        }

        private static void ReceiveDecrypted(Socket socket, byte[] buffer)
        {
            while (socket.Available < buffer.Length) { }
            socket.Receive(buffer);

            buffer = Cryptography.Decrypt(buffer, Server.Encryption.Key, Server.Encryption.IV);
        }

        private static void Receive(Socket socket, byte[] buffer)
        {
            while (socket.Available < buffer.Length) { }
            socket.Receive(buffer);
        }

        public static void StartAsync (int port, int bufferSize, Download download)
        {
            Downloads.Add(Task.Run(() => { Start(port, bufferSize, download); }));      
        }

        private static void Start(int port, int bufferSize, Download download)
        {
            try
            {
                var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(1);

                var connection = listener.Accept();
                connection.ReceiveBufferSize = 1000000;

                while (true)
                {
                    var length = BitConverter.ToInt32(Receive(connection, 4), 0);

                    if (length == -1)
                    {
                        break;
                    }
                    else
                    {
                        download.Receive(Receive(connection, length));
                    }
                }

                download.Finish();
                connection.Close();
            }
            catch (Exception e)
            {
                Server.Print("Download error: " + e.ToString());
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