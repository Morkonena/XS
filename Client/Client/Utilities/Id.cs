using System;

namespace XC.Utilities
{
    class Uid
    {
        public long Value;

        public Uid() {}
        public Uid(long value)
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
    }
}
