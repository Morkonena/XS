using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using XSC;
using XCB;

namespace Server
{
    class CommandProcess : IDevice
    {
        public static List<CommandProcess> Running = new List<CommandProcess>();
        public static List<CommandInfo> Commands = new List<CommandInfo>();

        public Command Device;

        public string Name; // 'Process Leak'
        public Uid Id;

        public CommandProcess (string name)
        {
            Id = Uid.Generate();
            Name = name;         
        }

        public static void Initialize ()
        {
            var compiler = (string)Database.Prefrences.GetValue("Compiler");

            if (compiler == null || !File.Exists(compiler))
            {
                Start:

                Console.Clear();
                Console.Write("Visual C# Compiler: ");

                if (string.IsNullOrEmpty((compiler = Console.ReadLine())) || !File.Exists(compiler))
                {
                    Console.WriteLine("Unable to access the compiler!");
                    Utility.PressAnyKey();
                    goto Start;
                }
                
                Database.Prefrences.SetValue("Compiler", compiler);
                Console.Clear();
            }

            CommandBase.Initialize(compiler, Database.Root);
            var information = new DirectoryInfo(Database.Commands);

            foreach (var folder in information.GetDirectories())
            {
                try
                {
                    Commands.Add(new CommandInfo(folder.Name, File.ReadAllText(string.Format(CommandBase.FormatDescription, folder.Name))));
                }
                catch (Exception e)
                {
                    Server.Print(string.Format("Corruption {0}: {1}", folder.FullName, e.ToString()));
                }
            }
        }

        public static CommandProcess GetProcess (Uid id)
        {
            return Running.Find(process => process.Id == id);
        }

        public static void Start (string name)
        {
            var process = new CommandProcess(name);
            Running.Add(process);

            Server.Print(string.Format("Start {0}, {1}", name, process.Id.ToString()));
            Server.Send(MessageType.Succeeded, new StartRespond(process.Id));

            try
            {
                process.Device = CommandBase.Execute(name, process);
            }
            catch (Exception e)
            {
                Server.Print(string.Format("Start error: {0}", e.ToString()));
                Server.Send(MessageType.Failed, null);
                return;
            }
        }

        public static void Create (CreateRequest request)
        {          
            Server.Print(string.Format("Create {0}, {1}", request.Name, request.Description));
            
            try
            {
                for (var i = 0; i < request.Files.Length; i++)
                {
                    File.WriteAllBytes(Database.Temporary + "\\" + request.Files[i].Filename, request.Files[i].Bytes);
                }
            }
            catch
            {
                Server.Print("Decompression error");
                Server.Send(MessageType.Failed, Encoding.UTF8.GetBytes("Lähdekoodien purkaminen epäonnistui!"));
                return;
            }

            var result = CommandBase.Create(request.Name, request.Description, Database.Temporary);

            if (!result.Succeeded)
            {
                Server.Print("Compile error: " + result.Output);
                Server.Send(MessageType.Failed, Encoding.UTF8.GetBytes(result.Output));
                return;
            }

            var executable = Commands.Find(i => i.Name == request.Name);

            if (executable != null)
            {
                executable.Name = request.Name;
                executable.Description = request.Description;
            }
            else
            {
                Commands.Add(new CommandInfo(request.Name, request.Description));
            }

            Server.Send(MessageType.Succeeded, Encoding.UTF8.GetBytes(result.Output));
        }

        public static void Delete (DeleteRequest request)
        {     
            var folder = new DirectoryInfo(string.Format(CommandBase.FormatCommandFolder, request.Name));

            if (folder.Exists)
            {
                folder.Delete(true);

                for (var i = Commands.Count - 1; i >= 0; i--)
                {
                    if (Commands[i].Name == request.Name)
                    {
                        Commands.RemoveAt(i);
                        break;
                    }
                }

                for (var i = Running.Count - 1; i >= 0; i--)
                {
                    if (Running[i].Name == request.Name)
                    {
                        Running[i].OnExit();
                    }
                }
            }

            Server.Print(string.Format("Delete {0}", request.Name));
        }

        public static void Finish ()
        {
            try
            {
                Directory.Delete(Database.Temporary, true);
                Directory.CreateDirectory(Database.Temporary);
            }
            catch (Exception e)
            {
                Server.Print("Post-create error (Fatal)");
                Server.Send(MessageType.Error, "Väliaikaiskansioiden tyhjentäminen epäonnistui:\n\n" + e.ToString());
            }
        }

        public void OnInput (string input)
        {
            Device.OnInput(input);
        }

        public void OnExit()
        {
            Device.OnExit();
        }

        public void OnError(Exception e)
        {
            Server.Print("Command error: " + e.ToString());
            Server.Send(MessageType.Error, e.ToString());
        }

        public void OnPrint (string text)
        {
            Server.Send(MessageType.Console, new ConsoleMessage(Id, text));
        }

        public void OnUpload (Upload upload)
        {
            lock (UploadManager.Available)
            {
                if (UploadManager.Available.Count > 0)
                {
                    var port = UploadManager.Available.Dequeue();
                    UploadManager.StartAsync(port, 100000, upload);

                    Server.Send(MessageType.Download, new DownloadStartRequest(Id, port, upload.Filename));
                }
            }
        }

        public void OnDownload (Download download)
        {
            lock (DownloadManager.Available)
            {
                if (DownloadManager.Available.Count > 0)
                {
                    var port = DownloadManager.Available.Dequeue();
                    DownloadManager.StartAsync(port, 100000, download);

                    Server.Send(MessageType.Upload, new UploadStartRequest(Id, port, download.Filename));
                }
            }          
        }
    }
}
