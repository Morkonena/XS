using XOS;

namespace XC.Utilities
{
    class Utility
    {
        public static T Convert<T>(byte[] Bytes)
        {
            return Deserilization.Deserialize<T>(Bytes);
        }

        public static byte[] Convert(object Object)
        {
            return Serilization.Serialize(Object);
        }
    }
}