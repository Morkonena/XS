namespace Server
{
    class Processor
    {
        public static void Process (ClientMessage message)
        {
            if (!Server.Logged)
            {            
                Login.Process(Utility.Convert<LoginRequest>(message.Data));
                return;
            }

            switch (message.Type)
            {
                case MessageType.Disconnect:
                {
                    Server.Disconnect(false);
                    break;
                }
                case MessageType.Create:
                {
                    var request = Utility.Convert<CreateRequest>(message.Data);

                    CommandProcess.Create(request);
                    CommandProcess.Finish();
                    break;
                }
                case MessageType.Delete:
                {
                    var request = Utility.Convert<DeleteRequest>(message.Data);

                    CommandProcess.Delete(request);
                    break;
                }
                case MessageType.Start:
                {
                    var request = Utility.Convert<StartRequest>(message.Data);

                    CommandProcess.Start(request.Name);
                    break;
                }
                case MessageType.Stop:
                {
                    var request = Utility.Convert<StopRequest>(message.Data);

                    CommandProcess.GetProcess(request.Id).OnExit();
                    break;
                }
                case MessageType.Console:
                {
                    var request = Utility.Convert<ConsoleMessage>(message.Data);

                    CommandProcess.GetProcess(request.Id).OnInput(request.Text);
                    break;
                }
                default:
                {
                    Server.Disconnect(true);
                    break;
                }
            }
        }
    }
}
