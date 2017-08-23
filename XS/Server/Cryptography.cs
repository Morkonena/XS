using System;
using System.Text;
using System.Security.Cryptography;

namespace Server
{
    class Cryptography
    {
        public static RandomNumberGenerator Generator = RandomNumberGenerator.Create();
        public static byte[] GeneralKey;

        public static void Initialize (char[] buffer)
        {
            GeneralKey = Encoding.UTF8.GetBytes(buffer);
        }

        public static byte[] Encrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            return Crypt(buffer, key, iv, true);
        }

        public static byte[] Decrypt (byte[] buffer, byte[] key, byte[] iv)
        {
            return Crypt(buffer, key, iv, false);
        }

        private static byte[] Crypt (byte[] buffer, byte[] key, byte[] iv, bool encrypt)
        {
            var device = new AesManaged()
            {
                BlockSize = 128,
                KeySize = 256,
                Key = key,
                IV = iv,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            if (encrypt)
            {
                return device.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length);
            }
            else
            {
                return device.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length);
            }
        }

        public static byte[] Generate (int length)
        {
            var bytes = new byte[length];
            Generator.GetBytes(bytes);

            return bytes;
        }

        public static void SendEncryptionInformation ()
        {
            var iv = Generate(16);

            var device = new AesManaged()
            {
                BlockSize = 128,
                KeySize = 128,
                Key = GeneralKey,
                IV = iv,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            var buffer = device.CreateEncryptor().TransformFinalBlock(Server.Key, 0, Server.Key.Length);

            Server.Device.Send(BitConverter.GetBytes(buffer.Length));
            Server.Device.Send(iv);
            Server.Device.Send(buffer);
        }
    }
}
