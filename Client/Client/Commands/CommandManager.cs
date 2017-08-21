using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Client.Activities;
using Client.Utilities;
using Client.Downloading;
using Client.Uploading;

namespace Client.Commands
{
    class CommandManager
    {
        public static bool Process (Base application, ServerMessage message)
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

                    DownloadManager.StartAsync(application, request.Port, 100000, request.Filename);             
                    return true;
                }
                case MessageType.Upload:
                {
                    var request = Utility.Convert<UploadStartRequest>(message.Data);

                    UploadManager.StartAsync(application, request.Port, 100000, request.Filename);
                    return true;
                }
            }

            return false;
        }
    }
}