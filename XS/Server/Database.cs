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

            if ((Prefrences = Registry.CurrentUser.OpenSubKey("Software\\XS", true)) == null)
            {
                Server.Print("Creating registry keys...");
                Prefrences = Registry.CurrentUser.CreateSubKey("Software\\XS");
            }               
        }
    }
}
