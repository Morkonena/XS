using System;
using XOS;

namespace Server
{
    class Utility
    {
        public static T Convert<T>(byte[] bytes)
        {
            return Deserilization.Deserialize<T>(bytes);
        }

        public static byte[] Convert (object obj)
        {
            return Serilization.Serialize(obj);
        }

        public static void PressAnyKey ()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
