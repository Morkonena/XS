using System;
using Client.Utilities;

namespace Client.Downloading
{
    enum DownloadStartResult
    {
        Error,
        Success
    }

    class DownloadStartRespond
    {
        public DownloadStartResult Result;

        public DownloadStartRespond (DownloadStartResult result)
        {
            Result = result;
        }
    }
}