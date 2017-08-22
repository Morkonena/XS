namespace XC.Utilities
{
    class ClientMessage
    {
        public MessageType Type;
        public byte[] Data;

        public ClientMessage (MessageType type, object data)
        {
            Type = type;
            Data = Utility.Convert((data ?? new byte[] { }));
        }
    }
}