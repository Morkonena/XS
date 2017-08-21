namespace Server
{
    public class CommandInfo
    {
        public string Name;
        public string Description;

        public CommandInfo (string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    public class LoginRespond
    {
        public CommandInfo[] Commands = CommandProcess.Commands.ToArray();
    }
}
