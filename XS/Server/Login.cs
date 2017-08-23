using System;
using System.Text;

namespace Server
{
    class Login
    {
        public static byte[] Password;

        public static void Initialize ()
        {
            if ((Password = (byte[])Database.Prefrences.GetValue("Password")) == null)
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

                Database.Prefrences.SetValue("Password", Password);
                Console.Clear();
            }
        }

        public static void Process (LoginRequest request)
        {
            Server.Print("Login");

            var bytes = Encoding.UTF8.GetBytes(request.Password);
            
            if (bytes.Length != Password.Length)
            {
                Server.Print("Denied");
                Server.Send(MessageType.Failed, null);
                return;
            }

            for (var i = 0; i < Password.Length; i++)
            {
                if (bytes[i] != Password[i])
                {
                    Server.Print("Denied");
                    Server.Send(MessageType.Failed, null);
                    return;
                }
            }

            Server.Print("Granted");

            Server.Send(MessageType.Succeeded, new LoginRespond());
            Server.Logged = true;
        }
    }
}
