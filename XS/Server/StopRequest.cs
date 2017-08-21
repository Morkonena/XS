namespace Server
{
    class StopRequest
    {
        public Uid Id;

        public StopRequest () {}
        public StopRequest (Uid id)
        {
            Id = id;
        }
    }
}
