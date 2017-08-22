using Android.App;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Client.Commands;
using Client.Login;
using Client.Uploading;
using Client.Utilities;

namespace Client.Activities
{
    class Connection
    {
        public static Socket Device { get; set; }
        public static IPEndPoint ConnectionInfo { get; set; }
        public static EncryptionInformation Encryption { get; set; }

        public static Action<MessageType, byte[]> Callback { get; set; }

        public static int BufferSize { get { return Device.SendBufferSize; } }

        public static void Initialize (Base application)
        {
            new Thread(() =>
            {
                Device = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Device.Connect(new IPEndPoint(IPAddress.Parse("XX.XXX.XXX.XXX"), 1111));

                ConnectionInfo = Device.RemoteEndPoint as IPEndPoint;
                
                // Vastaanota suojaustiedot ja tarkista, että se onnistui
                if (!InitializeEncryption(application))
                {
                    return;
                }

                // Käynnistä kirjautumisaktiviteetti
                application.RunOnUiThread(() => { new LoginActivity(application); });

                while (true)
                {
                    try
                    {
                        if (Device.Available >= 4)
                        {
                            // Odota kunnes uusi viesti on saapumassa
                            while (Device.Available < 4)
                            {
                                Thread.Sleep(100);
                            }

                            // Tulevan viestin koko
                            var length = BitConverter.ToInt32(Receive(4), 0);

                            // Tarkista, jos viesti onkin vain 'Ping'
                            if (length == Base.PingCode)
                            {
                                Connection.Device.Send(BitConverter.GetBytes(-1));
                                continue;
                            }
                            else if (length == Base.DisconnectionCode)
                            {
                                Disconnect(false);
                                ShowDialog(application, "Palvelin katkaisi yhteyden", "Yhteys katkaistu");
                                return;
                            }

                            // Odota kunnes kaikki tieto on saapunut
                            while (Device.Available < length)
                            {
                                Thread.Sleep(100);
                            }

                            // Pura tavujen salaus ja muunna ne 'ServerMessage' objektiksi
                            var message = Cryptography.DecryptMessage(Receive(length));

                            if (!CommandManager.Process(application, message) && Callback != null)
                            {
                                application.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        Callback.Invoke(message.Type, message.Data);
                                    }
                                    catch (Exception e)
                                    {
                                        Disconnect(true);
                                        ShowDialog(application, "Vastauksen suorittaminen epäonnistui: " + e.ToString(), "Yhteys katkaistu");
                                    }                                  
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {     
                        // Odottamaton kaatuminen: Sulje yhteys ja näytä virhe raportti
                        Disconnect(true);
                        OnUnexceptedCrash(application, e);
                        break;
                    }
                }

            }).Start();
        }

        private static void ShowDialog (Base b, string text, string title, bool cancelable = false)
        {
            var Builder = new AlertDialog.Builder(b);
            Builder.SetTitle(title);
            Builder.SetMessage(text);
            Builder.SetCancelable(cancelable);
            Builder.SetPositiveButton("OK", (Sender, Arguments) => { });
            Builder.Show();
        }

        private static void OnUnexceptedCrash (Base application, Exception error)
        {
            application.RunOnUiThread(() =>
            {
                ShowDialog(application, error.ToString(), "Yhteys katkaistu");
            });
        }

        private static byte[] Receive (int length)
        {
            // Vastaanota 'Length' määrä tavuja
            var buffer = new byte[length];
            Device.Receive(buffer);

            return buffer;
        }

        private static bool InitializeEncryption (Base client)
        {
            try
            {
                while (Device.Available < 4) ;

                var length = BitConverter.ToInt32(Receive(4), 0);
                while (Device.Available < length) ;

                Encryption = Utility.Convert<EncryptionInformation>(Cryptography.Decrypt(Receive(length), Cryptography.GeneralKeyBytes, Cryptography.GeneralIVBytes));
            }
            catch (Exception e)
            {
                var builder = new AlertDialog.Builder(client);
                builder.SetTitle("Virhe");
                builder.SetMessage("Suojaustietojen vastaanottaminen epäonnistui: " + e.ToString());
                builder.SetCancelable(false);

                builder.SetPositiveButton("OK", (Sender, Arguments) => { });

                builder.Show();

                return false;
            }

            return (Encryption != null);
        }

        public static bool Send (MessageType type, object data)
        {          
            try
            {
                var message = new ClientMessage(type);
                byte[] buffer;

                if ((data != null && (message.Data = Utility.Convert(data)) == null) ||
                    (buffer = Cryptography.EncryptMessage(message)) == null)
                {
                    return false;
                }

                Device.Send(BitConverter.GetBytes(buffer.Length));
                Device.Send(buffer);
            }
            catch (Exception e)
            {
                Disconnect(false);
                return false;
            }

            return true;
        }

        public static void Disconnect(bool notify)
        {
            // Tarkista, jos yhteyden katkaisemisesta halutaan ilmoittaa
            if (notify)
            {
                try
                {
                    Device.Send(BitConverter.GetBytes(Base.DisconnectionCode)); // Lähetä yhteyden katkaisu käsky
                }
                catch {}              
            }
                         
            try
            {      
                Device.Close(); // Sulje yhteys
            }
            catch {}

            Device = null; // Poista yhteys
        }
    }
}