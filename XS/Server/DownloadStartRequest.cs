namespace Server
{
    class DownloadStartRequest
    {
        public Uid Id;
        public int Port;
        public string Filename;

        public DownloadStartRequest (Uid id, int port, string filename)
        {
            Id = id;
            Port = port;
            Filename = filename;
        }
    }
}
