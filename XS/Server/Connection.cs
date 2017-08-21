using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    class Connection
    {
        private Thread Listener;
        private Socket Client;

        private DateTime Sent;
        private DateTime Received;

        private bool Running = true;

        public Connection(Socket client)
        {
            Client = client;        
        }

        public void Initialize ()
        {
            var information = (IPEndPoint)Client.RemoteEndPoint;

            Server.Print(string.Format("Accept {0}:{1}", information.Address, information.Port));
            Cryptography.SendEncryptionInformation();

            Listener = new Thread(StartListening);
            Listener.Start();
        }

        public bool Ping ()
        {
            if ((Sent - Received).Minutes >= 1)
            {
                Server.Disconnect(false);
                return false;
            }
            else if ((DateTime.Now - Sent).Seconds >= 10)
            {
                try
                {
                    Server.Ping();
                }
                catch
                {
                    Server.Disconnect(false);
                    return false;
                }
               
                Sent = DateTime.Now;
            }

            return true;
        }

        public void StartListening()
        {
            Sent = DateTime.Now;
            Received = DateTime.Now;

            Ping();

            while (Running)
            {
                try
                {
                    if (Ping() && Client.Available >= 4)
                    {
                        Received = DateTime.Now;

                        var length = BitConverter.ToInt32(Receive(4), 0);

                        if (length == Server.PingCode)
                        {
                            continue;
                        }
                        else if (length == Server.DisconnectCode)
                        {
                            Server.Disconnect(false);
                            return;
                        }
                        else if (length < 0)
                        {
                            Server.Print("Processing error: Size");
                            Server.Disconnect();
                            return;
                        }

                        while (Client.Available < length) {}
                        var buffer = Receive(length);

                        if ((buffer = Cryptography.Decrypt(buffer, Server.Encryption.Key, Server.Encryption.IV)) == null)
                        {
                            Server.Disconnect();
                            return;
                        }

                        try
                        {
                            Processor.Process(Utility.Convert<ClientMessage>(buffer));
                        }
                        catch (Exception e)
                        {
                            Server.Print("Processing error: " + e.ToString());
                            Server.Disconnect();
                            return;
                        }                   
                    }
                }
                catch (Exception e)
                {
                    Server.Print("Unexpected error: " + e.ToString());
                    Server.Disconnect();
                    return;
                }

                Thread.Sleep(100);
            }
        }

        public void Send(byte[] Bytes)
        {
            Client.Send(Bytes);
        }

        public void Disconnect (bool notify)
        {
            Running = false;
           
            if (notify)
            {
                try
                {
                    Client.Send(BitConverter.GetBytes(Server.DisconnectCode));
                }
                catch {}              
            }
           
            try
            {
                Client.Close();
            }
            catch {}
        }

        public byte[] Receive(int Length)
        {
            byte[] Buffer = new byte[Length];
            Client.Receive(Buffer);

            return Buffer;
        }
    }
}
