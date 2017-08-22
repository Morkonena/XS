using System;
using XC.Utilities;

namespace XC.Commands
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