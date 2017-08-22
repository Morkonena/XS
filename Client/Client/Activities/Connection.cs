using Android.App;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using XC.Commands;
using XC.Login;
using XC.Uploading;
using XC.Utilities;

namespace XC.Activities
{
    class Connection
    {
        public static Socket Device { get; set; }
        public static IPEndPoint ConnectionInfo { get; set; }
        public static EncryptionInformation Encryption { get; set; }

        public static Action<MessageType, byte[]> Callback { get; set; }

        public static void Initialize (RootActivity root)
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Device = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        Device.Connect(ConnectionInfo);

                        InitializeEncryption();
                    }
                    catch
                    {
                        Thread.Sleep(5000);

                        root.ShowToast("Yritetään uudestaan...");
                        continue;
                    }

                    break;
                }

                try
                {
                    root.RunOnUiThread(() => new LoginActivity(root));
                }
                catch (Exception e)
                {
                    root.ShowDialog(e.ToString(), "Virhe");
                    return;
                }
             
                while (true)
                {
                    try
                    {
                        if (Device.Available >= 4)
                        {
                            var length = BitConverter.ToInt32(Receive(4), 0);

                            if (length == RootActivity.PingCode)
                            {
                                Connection.Device.Send(BitConverter.GetBytes(-1));
                                continue;
                            }
                            else if (length == RootActivity.DisconnectionCode)
                            {
                                Disconnect(false);

                                root.ShowToast("Yhteys katkaistu!");
                                return;
                            }

                            while (Device.Available < length)
                            {
                                Thread.Sleep(100);
                            }

                            var message = Cryptography.DecryptMessage(Receive(length));

                            if (!CommandManager.Process(root, message) && Callback != null)
                            {
                                root.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        Callback.Invoke(message.Type, message.Data);
                                    }
                                    catch (Exception e)
                                    {
                                        Disconnect(true);
                                        root.ShowDialog(e.ToString(), "Virhe");
                                    }                                  
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {     
                        Disconnect(true);
                        OnUnexceptedCrash(root, e);
                        break;
                    }
                }

            }).Start();
        }

        private static void OnUnexceptedCrash (RootActivity root, Exception e)
        {
            root.RunOnUiThread(() =>
            {
                root.ShowDialog(e.ToString(), "Virhe");
            });
        }

        private static byte[] Receive (int length)
        {
            var buffer = new byte[length];
            Device.Receive(buffer);

            return buffer;
        }

        private static void InitializeEncryption ()
        {
            while (Device.Available < 4) {}

            var length = BitConverter.ToInt32(Receive(4), 0);
            while (Device.Available < length) {}

            Encryption = Utility.Convert<EncryptionInformation>(Cryptography.Decrypt(Receive(length), Cryptography.GK, null));
        }

        public static void Send (MessageType type, object data)
        {
            try
            {
                var buffer = Cryptography.EncryptMessage(new ClientMessage(type, data));

                Device.Send(BitConverter.GetBytes(buffer.Length));
                Device.Send(buffer);
            }
            catch (Exception e)
            {
                Disconnect(false);
            }        
        }

        public static void Disconnect(bool notify)
        {
            if (notify)
            {
                try
                {
                    Device.Send(BitConverter.GetBytes(RootActivity.DisconnectionCode)); 
                }
                catch {}              
            }
                         
            try
            {      
                Device.Close();
            }
            catch {}

            Device = null;
        }
    }
}