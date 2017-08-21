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
        public static EncryptionInformation Encryption = new EncryptionInformation();

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
                Cryptography.Initialize();
                CommandProcess.Initialize();

                Uid.Initialize();

                DownloadManager.Initialize(1000, 10);
                UploadManager.Initialize(1000, 10);

                if (!Login.Initialize())
                {
                    Shutdown(60, "Initialization error: Login");
                }                
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
            Encryption = new EncryptionInformation();

            while (CommandProcess.Running.Count > 0)
            {
                CommandProcess.Running[0].OnExit();
            }

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

        public static void Send (MessageType type, byte[] data)
        {
            var message = new ServerMessage(type, data);
            var bytes = Utility.Convert(message);

            if ((bytes = Cryptography.Encrypt(bytes, Encryption.Key, Encryption.IV)) == null)
            {
                Disconnect();
                return;
            }

            try
            {
                Device.Send(BitConverter.GetBytes(bytes.Length));
                Device.Send(bytes);
            }
            catch
            {
                Disconnect(false);
            }
        }

        public static void Send (MessageType type, object data)
        {
            var message = new ServerMessage(type, data);
            var bytes = Utility.Convert(message);

            if ((bytes = Cryptography.Encrypt(bytes, Encryption.Key, Encryption.IV)) == null)
            {
                Disconnect();
                return;
            }

            try
            {
                Device.Send(BitConverter.GetBytes(bytes.Length));
                Device.Send(bytes);
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
