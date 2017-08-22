namespace XC.Commands
{
    class VirtualFile
    {
        public string Filename;
        public byte[] Bytes;

        public VirtualFile (string filename, byte[] bytes)
        {
            Filename = filename;
            Bytes = bytes;
        }
    }

    class CreateRequest
    {
        public VirtualFile[] Files;

        public string Name;
        public string Description;
    }
}