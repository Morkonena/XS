using System.Text;
using System.Security.Cryptography;

using XC.Activities;

namespace XC.Utilities
{
    class Cryptography
    {
        public static RandomNumberGenerator Generator = RandomNumberGenerator.Create();
        public static byte[] GeneralKey;

        public static void Initialize (string key)
        {
            GeneralKey = Encoding.UTF8.GetBytes(key);
        }

        public static byte[] Generate(int length)
        {
            var bytes = new byte[length];
            Generator.GetBytes(bytes);

            return bytes;
        }

        public static byte[] Encrypt (byte[] buffer, byte[] key, byte[] iv)
        {
            return Crypt(buffer, key, iv, true);
        }

        public static byte[] Decrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            return Crypt(buffer, key, iv, false);
        }

        private static byte[] Crypt (byte[] buffer, byte[] key, byte[] iv, bool encrypt)
        {
            var device = new AesManaged();
            device.BlockSize = 128;
            device.KeySize = 256;
            device.Key = key;
            device.IV = iv;
            device.Mode = CipherMode.CBC;
            device.Padding = PaddingMode.PKCS7;

            if (encrypt)
            {
                return device.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length);
            }
            else
            {
                return device.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length);
            }
        }
    }
}