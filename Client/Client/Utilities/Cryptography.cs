using System.Text;
using System.Security.Cryptography;

using XC.Activities;

namespace XC.Utilities
{
    class Cryptography
    {
        public static byte[] GK;

        public static void Initialize (string key)
        {
            GK = Encoding.UTF8.GetBytes(key);
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

        public static ServerMessage DecryptMessage (byte[] buffer)
        {
            return Utility.Convert<ServerMessage>(Decrypt(buffer, Connection.Encryption.Key, Connection.Encryption.IV));
        }

        public static byte[] EncryptMessage (ClientMessage message)
        {
            return Encrypt(Utility.Convert(message), Connection.Encryption.Key, Connection.Encryption.IV);
        }
    }
}