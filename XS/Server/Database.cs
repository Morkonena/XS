using System;
using System.IO;
using Microsoft.Win32;

namespace Server
{
    class Database
    {
        public const string Root = "C:\\Server";
        public const string Commands = "C:\\Server\\Commands";
        public const string Temporary = "C:\\Server\\Temporary";

        public static RegistryKey Prefrences;

        public static string CreateKey ()
        {
            var generator = new Random(BitConverter.ToInt32(Cryptography.Generate(4), 0));
            var symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

            var key = string.Empty;

            for (var i = 0; i < 16; i++)
            {
                if (i % 4 == 0)
                {
                    key += '-';
                }

                key += symbols[generator.Next(0, symbols.Length)]; ; 
            }

            return key;
        }

        public static void Initialize ()
        {
            if (!Directory.Exists(Root))
            {
                Server.Print("Creating root folder...");
                Directory.CreateDirectory(Root);
            }

            if (!Directory.Exists(Commands))
            {
                Server.Print("Creating command folder...");
                Directory.CreateDirectory(Commands);
            }

            if (!Directory.Exists(Temporary))
            {
                Server.Print("Creating temporary folder...");
                Directory.CreateDirectory(Temporary);
            }

            string key;

            if ((Prefrences = Registry.CurrentUser.OpenSubKey("Software\\XS", true)) == null)
            {
                Server.Print("Creating registry keys...");
                Prefrences = Registry.CurrentUser.CreateSubKey("Software\\XS");

                Server.Print("Generating general key...");

                Prefrences.SetValue("Key", (key = CreateKey()));
            }
            else
            {
                key = Prefrences.GetValue("Key", null) as string;
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("General key was invalid!");
            }

            Console.Title = key;
            Cryptography.Initialize(key.Replace("-", "").ToCharArray());
        }
    }
}
