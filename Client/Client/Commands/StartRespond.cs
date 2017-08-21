using System;
using Client.Utilities;

namespace Client.Commands
{
    class StartRespond
    {
        public Uid Id;

        public StartRespond() { }
        public StartRespond(Uid id)
        {
            Id = id;
        }
    }
}
