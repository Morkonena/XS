using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using XC.Activities;
using XC.Utilities;
using XC.Downloading;
using XC.Uploading;

namespace XC.Commands
{
    class CommandManager
    {
        public static bool Process (RootActivity application, ServerMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Console:
                {
                    var request = Utility.Convert<ConsoleMessage>(message.Data);

                    CommandActivity.GetProcess(request.Id).Append(request.Text);
                    return true;
                }
                case MessageType.Download:
                {
                    var request = Utility.Convert<DownloadStartRequest>(message.Data);

                    DownloadManager.StartAsync(application, request.Port, 1000000, request.Filename);             
                    return true;
                }
                case MessageType.Upload:
                {
                    var request = Utility.Convert<UploadStartRequest>(message.Data);

                    UploadManager.StartAsync(application, request.Port, 1000000, request.Filename);
                    return true;
                }
            }

            return false;
        }
    }
}