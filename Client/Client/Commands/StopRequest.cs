using System;
using Client.Utilities;

namespace Client.Commands
{
    class StopRequest
    {
        public Uid Id;

        public StopRequest (Uid id)
        {
            Id = id;
        }
    }
}