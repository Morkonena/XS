using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        public const int PingCode = -1;
        public const int DisconnectCode = -2;

        public static Connection Device;
        public static byte[] Key;

        public static bool Logged = false;

        public static void Listen ()
        {
            Socket listener = null;

            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, 1111));
                listener.Listen(1);
            }
            catch (Exception e)
            {
                Shutdown(60, "Binding error: " + e.ToString());
                return;
            }
            
            while (true)
            {
                Print("Listening...");

                try
                {
                    Device = new Connection(listener.Accept());
                    Device.Initialize();
                }
                catch (Exception e)
                {
                    Print("Connection error: " + e.ToString());
                    Disconnect(true);
                }

                while (Device != null) {}
            }         
        }

        public static void Main (string[] arguments)
        {
            Console.Title = "XS";

            try
            {
                Database.Initialize();
                CommandProcess.Initialize();

                Uid.Initialize();

                DownloadManager.Initialize(1000, 10);
                UploadManager.Initialize(1000, 10);

                Login.Initialize();
            }
            catch (Exception e)
            {
                Shutdown(60, "Initialization error: " + e.ToString());
                return;
            }

            Task.Run((Action)Listen).Wait();
        }

        public static void Disconnect (bool notify = true)
        {
            Logged = false;
            Key = Cryptography.Generate(32);

            Task.Run(() =>
            {
                foreach (var process in CommandProcess.Running)
                {
                    try
                    {
                        process.OnExit();
                    }
                    catch {}
                }

                CommandProcess.Running.Clear();
            });
            
            Print("Disconnected");

            if (Device != null)
            {
                Device.Disconnect(notify);
                Device = null;
            }           
        }

        public static void Print (string text)
        {
            Console.WriteLine(string.Format("[{0}:{1}:{2}]: {3}", 
                (DateTime.Now.Hour   < 10 ? "0" + DateTime.Now.Hour.ToString()   : DateTime.Now.Hour.ToString()),
                (DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString()),
                (DateTime.Now.Second < 10 ? "0" + DateTime.Now.Second.ToString() : DateTime.Now.Second.ToString()), text));
        }

        public static void Send (MessageType type, object data)
        {
            try
            {
                var buffer = Utility.Convert(new ServerMessage(type, data));
                var iv = Cryptography.Generate(16);

                buffer = Cryptography.Encrypt(buffer, Key, iv);

                Device.Send(BitConverter.GetBytes(buffer.Length));
                Device.Send(iv);
                Device.Send(buffer);
            }
            catch
            {
                Disconnect(false);
            }
        }

        public static void Ping ()
        {
            Device.Send(BitConverter.GetBytes(-1));
        }

        private static void Shutdown (int delay, string text)
        {
            for (var i = 0; i < 2; i++)
            {
                Console.Beep(2000, 500);
            }

            Print(text);
            Print(string.Format("Shutdown in {0} second(s)", delay.ToString()));
            
            Thread.Sleep(delay * 1000);
            Environment.Exit(1);
        }
    }
}
