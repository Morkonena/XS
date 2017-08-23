using Android.App;
using Android.Content;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
       
        public static byte[] Key { get; set; }

        public static Task Worker { get; set; }
        public static bool Running { get; set; } = true;

        public static Action<MessageType, byte[]> Callback { get; set; }

        public static void Initialize (RootActivity root)
        {
            root.SetContentView(Resource.Layout.Wait);

            Worker = Task.Run(() =>
            {
                try
                {
                    Device = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    var result = Device.BeginConnect(ConnectionInfo, null, null);

                    if (!result.AsyncWaitHandle.WaitOne(5000, true))
                    {
                        Device.Close();

                        OnConnectionFailed(root);
                        return;
                    }

                    InitializeEncryption();
                }
                catch
                {
                    OnConnectionFailed(root);
                    return;
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
             
                while (Running)
                {
                    try
                    {
                        if (Device.Available >= 4)
                        {
                            var length = BitConverter.ToInt32(Read(4), 0);

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

                            var iv = Receive(16);
                            var message = Utility.Convert<ServerMessage>(Cryptography.Decrypt(Receive(length), Key, iv));

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

            });
        }

        private static void OnConnectionFailed (RootActivity root)
        {
            var builder = new AlertDialog.Builder(root);
            builder.SetTitle("Yhdistäminen epäonnistui");
            builder.SetMessage("Haluatko muokata yhteysasetuksia vai yrittää uudestaan?");
            builder.SetCancelable(false);

            builder.SetPositiveButton("Yritä uudestaan", (sender, arguments) =>
            {
                Connection.Shutdown();
                Connection.Initialize(root);
            });

            builder.SetNegativeButton("Asetukset", (sender, arguments) =>
            {
                Connection.Shutdown();

                var prefrences = root.GetPreferences(FileCreationMode.Private);
                var port = prefrences.GetInt("Port", -1);

                LoginActivity.Setup(root, new string[] { prefrences.GetString("Address", string.Empty), (port == -1 ? string.Empty : port.ToString()), prefrences.GetString("Key", string.Empty) }, root.GetPreferences(FileCreationMode.Private).Edit(), () =>
                {
                    Connection.Initialize(root);
                });
            });

            root.RunOnUiThread(() => builder.Show());
        }

        private static void OnUnexceptedCrash (RootActivity root, Exception e)
        {
            root.RunOnUiThread(() =>
            {
                root.ShowDialog(e.ToString(), "Virhe");
            });
        }

        private static byte[] Read (int length)
        {
            var buffer = new byte[length];
            Device.Receive(buffer);

            return buffer;
        }

        private static byte[] Receive (int length)
        {
            while (Device.Available < length) { }

            var buffer = new byte[length];
            Device.Receive(buffer);

            return buffer;
        }

        private static void InitializeEncryption ()
        {
            var length = BitConverter.ToInt32(Receive(4), 0);
            var iv = Receive(16);

            var device = new AesManaged()
            {
                BlockSize = 128,
                KeySize = 128,
                Key = Cryptography.GeneralKey,
                IV = iv,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            Key = device.CreateDecryptor().TransformFinalBlock(Receive(length), 0, length);
        }

        public static void Send (MessageType type, object data)
        {
            try
            {
                var iv = Cryptography.Generate(16);
                var buffer = Cryptography.Encrypt(Utility.Convert(new ClientMessage(type, data)), Key, iv);

                Device.Send(BitConverter.GetBytes(buffer.Length));
                Device.Send(iv);
                Device.Send(buffer);
            }
            catch (Exception e)
            {
                Disconnect(false);
            }        
        }

        public static void Shutdown ()
        {
            Running = false;
            Worker.Wait();
            Running = true;
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