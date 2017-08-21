namespace Server
{
    class ServerMessage
    {
        public MessageType Type;  
        public byte[] Data;

        public ServerMessage (MessageType type, object data)
        {
            Type = type;
            Data = Utility.Convert(data);
        }

        public ServerMessage (MessageType type, byte[] data)
        {
            Type = type;
            Data = (data ?? (new byte[] { }));
        }
    }
}
