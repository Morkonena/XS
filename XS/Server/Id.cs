using System;

namespace Server
{
    class Uid
    {
        private static long Next = 0;

        public static void Initialize ()
        {
            Next = BitConverter.ToInt64(Cryptography.Generate(8), 0);
        }

        public static Uid Generate()
        {
            return new Uid(Next++);
        }

        public long Value;

        public Uid () {}
        public Uid (long value)
        {
            Value = value;
        }

        public static bool operator ==(Uid a, Uid b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Uid a, Uid b)
        {
            return a.Value != b.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
