using System;
using System.Text;

namespace Server
{
    class Login
    {
        public static byte[] Password;

        public static bool Initialize ()
        {
            if ((Password = (byte[])Database.Prefrences.GetValue("Password")) != null)
            {
                return (Password = Cryptography.Decrypt(Password, Cryptography.MasterKeyBytes, Cryptography.MasterIVBytes)) != null;
            }
            else
            {
                Start:

                Console.Clear();
                Console.Write("Password: ");

                var text = Console.ReadLine();

                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine("Password must be atleast one character long");
                    Utility.PressAnyKey();
                    goto Start;
                }

                Password = Encoding.UTF8.GetBytes(text);

                var buffer = new byte[Password.Length];
                Array.Copy(Password, 0, buffer, 0, buffer.Length);

                buffer = Cryptography.Encrypt(buffer, Cryptography.MasterKeyBytes, Cryptography.MasterIVBytes);

                Database.Prefrences.SetValue("Password", buffer);
                Console.Clear();
                return true;
            }
        }

        public static void Process (LoginRequest request)
        {
            Server.Print("Login");

            var bytes = Encoding.UTF8.GetBytes(request.Password);
            
            if (bytes.Length != Password.Length)
            {
                Server.Print("Access denied");
                Server.Send(MessageType.Failed, null);
                return;
            }

            for (int i = 0; i < Password.Length; i++)
            {
                if (bytes[i] != Password[i])
                {
                    Server.Print("Access denied");
                    Server.Send(MessageType.Failed, null);
                    return;
                }
            }

            Server.Print("Access granted");

            Server.Send(MessageType.Succeeded, new LoginRespond());
            Server.Logged = true;
        }
    }
}
