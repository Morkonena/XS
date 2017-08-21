namespace Server
{
    enum DownloadStartResult
    {
        Error,
        Success
    }

    class DownloadStartRespond
    {
        public Uid Process;
        public Uid Id;

        public DownloadStartResult Result;
    }
}
