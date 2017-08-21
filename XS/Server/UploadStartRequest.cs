namespace Server
{
    class UploadStartRequest
    {
        public Uid Id;
        public int Port;
        public string Filename;

        public UploadStartRequest (Uid id, int port, string filename)
        {
            Id = id;
            Port = port;
            Filename = filename;
        }
    }
}
