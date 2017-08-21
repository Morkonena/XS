namespace Server
{
    class ConsoleMessage
    {
        public Uid Id;
        public string Text;

        public ConsoleMessage () {}
        public ConsoleMessage (Uid id, string text)
        {
            Id = id;
            Text = text;
        }
    }
}
