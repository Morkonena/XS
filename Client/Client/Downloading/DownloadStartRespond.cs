using System;
using XC.Utilities;

namespace XC.Downloading
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