using System;
using Client.Utilities;

namespace Client.Uploading
{
    enum UploadStartResult
    {
        Error,
        Success
    }

    class UploadStartRespond
    {
        public UploadStartResult Result;

        public UploadStartRespond (UploadStartResult result)
        {
            Result = result;
        }
    }
}