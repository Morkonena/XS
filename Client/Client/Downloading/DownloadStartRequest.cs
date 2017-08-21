using System;
using Client.Utilities;

namespace Client.Downloading
{
    class DownloadStartRequest
    {
        public Uid Id;
        public int Port;
        public string Filename;
    }
}