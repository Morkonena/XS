using System;

namespace Server
{
    class EncryptionInformation
    {
        public byte[] Key;
        public byte[] IV;

        public EncryptionInformation ()
        {
            Key = Cryptography.Generate(32);
            IV = Cryptography.Generate(16);
        }
    }
}
