namespace Client.Utilities
{
    class ClientMessage
    {
        public MessageType Type;
        public byte[] Data;

        public ClientMessage (MessageType type)
        {
            Type = type;
        }
    }
}