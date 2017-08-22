using System;
using XC.Utilities;

namespace XC.Uploading
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