using System;
using System.Text;
using System.Security.Cryptography;

namespace Server
{
    class Cryptography
    {
        public static byte[] GK;

        public static RandomNumberGenerator Generator = RandomNumberGenerator.Create();

        public static void Initialize (char[] buffer)
        {
            GK = Encoding.UTF8.GetBytes(buffer);
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
            var bytes = Utility.Convert(Server.Encryption);
            bytes = Encrypt(bytes, GeneralKeyBytes, GeneralIVBytes);

            Server.Device.Send(BitConverter.GetBytes(bytes.Length));
            Server.Device.Send(bytes);
        }
    }
}
