using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;

using Java.Lang;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using XC.Commands;
using XC.Login;
using XC.Utilities;

namespace XC.Activities
{
    class EditorActivity : Alias
    {
        private EditText Field;
      
        private string Root;
        private string Name;

        /*
         * 
         *  Highlights
         * 
         */

        public static Typeface Font;

        public static Color NumberHighlightColor = new Color(180, 205, 170);
        public static Color CommentHighlightColor = new Color(90, 170, 75);
        public static Color TextHighlightColor = new Color(215, 160, 135);
        public static Color ObjectHighlightColor = new Color(80, 200, 175);
        public static Color DelegateHighlightColor = new Color(255, 110, 135);
        public static Color HighlightColor = new Color(85, 155, 215);

        public class Highlight
        {
            public string Text;
            public Color Color;

            public Highlight (string text, Color color)
            {
                Text = text;
                Color = color;
            }
        }

        public const int LongestHighlight = 9;

        public static Highlight[] Highlights = new Highlight[]
        {
            new Highlight("0", NumberHighlightColor),
            new Highlight("1", NumberHighlightColor),
            new Highlight("2", NumberHighlightColor),
            new Highlight("3", NumberHighlightColor),
            new Highlight("4", NumberHighlightColor),
            new Highlight("5", NumberHighlightColor),
            new Highlight("6", NumberHighlightColor),
            new Highlight("7", NumberHighlightColor),
            new Highlight("8", NumberHighlightColor),
            new Highlight("9", NumberHighlightColor),
            new Highlight("abstract", HighlightColor),
            new Highlight("as", HighlightColor),
            new Highlight("async", HighlightColor),
            new Highlight("base", HighlightColor),
            new Highlight("bool", HighlightColor),
            new Highlight("break", HighlightColor),
            new Highlight("byte", HighlightColor),
            new Highlight("case", HighlightColor),
            new Highlight("char", HighlightColor),
            new Highlight("class", HighlightColor),
            new Highlight("const", HighlightColor),
            new Highlight("continue", HighlightColor),
            new Highlight("decimal", HighlightColor),
            new Highlight("default", HighlightColor),
            new Highlight("delegate", HighlightColor),
            new Highlight("double", HighlightColor),
            new Highlight("do", HighlightColor),
            new Highlight("else", HighlightColor),
            new Highlight("enum", HighlightColor),
            new Highlight("extern", HighlightColor),
            new Highlight("false", HighlightColor),
            new Highlight("float", HighlightColor),
            new Highlight("for", HighlightColor),
            new Highlight("foreach", HighlightColor),
            new Highlight("goto", HighlightColor),
            new Highlight("if", HighlightColor),
            new Highlight("in", HighlightColor),
            new Highlight("is", HighlightColor),
            new Highlight("int", HighlightColor),
            new Highlight("interface", HighlightColor),
            new Highlight("internal", HighlightColor),
            new Highlight("long", HighlightColor),
            new Highlight("namespace", HighlightColor),
            new Highlight("null", HighlightColor),
            new Highlight("object", HighlightColor),
            new Highlight("override", HighlightColor),
            new Highlight("params", HighlightColor),
            new Highlight("private", HighlightColor),
            new Highlight("protected", HighlightColor),
            new Highlight("public", HighlightColor),
            new Highlight("new", HighlightColor),
            new Highlight("return", HighlightColor),
            new Highlight("short", HighlightColor),
            new Highlight("static", HighlightColor),
            new Highlight("string", HighlightColor),
            new Highlight("struct", HighlightColor),
            new Highlight("switch", HighlightColor),
            new Highlight("throw", HighlightColor),
            new Highlight("true", HighlightColor),
            new Highlight("uint", HighlightColor),
            new Highlight("ulong", HighlightColor),
            new Highlight("ushort", HighlightColor),
            new Highlight("using", HighlightColor),
            new Highlight("var", HighlightColor),
            new Highlight("virtual", HighlightColor),
            new Highlight("void", HighlightColor),
            new Highlight("while", HighlightColor),
            new Highlight("Command", ObjectHighlightColor),
            new Highlight("UploadStream", ObjectHighlightColor),
            new Highlight("DownloadStream", ObjectHighlightColor)
        };

        /*
         * 
         *  Text
         *
         */

        public const string CommandStartFile = "using XSC;" + "\n" +
                                               "\n" +
                                               "public class Main : Command" + "\n" +
                                               "{" + "\n" +
                                               "   public override void OnStart ()" + "\n" +
                                               "   {" + "\n" +
                                               "      " + "\n" +
                                               "   }" + "\n" +
                                               "\n" +
                                               "   public override void OnInput (string input)" + "\n" +
                                               "   {" + "\n" +
                                               "      " + "\n" +
                                               "   }" + "\n" +
                                               "\n" +
                                               "   public override void OnExit ()" + "\n" +
                                               "   {" + "\n" +
                                               "      " + "\n" +
                                               "   }" + "\n" +
                                               "}";

        /*
         * 
         *  Functions
         *
         */

        public static void Initialize (RootActivity root)
        {         
            Font = Typeface.CreateFromAsset(root.Assets, "Consolas.ttf"); // Määritä fontiksi 'Consolas'
        }

        public EditorActivity (RootActivity activity, string root) : base(activity)
        {
            Root = root;
            Start();
        }

        private void Start ()
        {
            SetContentView(Resource.Layout.Editor);

            Field = Find<EditText>(Resource.Id.Editor_Text);
            Field.SetTypeface(Font, TypefaceStyle.Normal);
           
            var start = 0;
            var count = 0;

            Field.BeforeTextChanged += (sender, arguments) =>
            {
                try
                {
                    start = arguments.Start;
                    count = arguments.AfterCount;
                    
                    var spans = Field.EditableText.GetSpans(start, start + arguments.BeforeCount, Class.FromType(typeof(ForegroundColorSpan)));

                    foreach (var span in spans)
                    {
                        Field.EditableText.RemoveSpan(span);
                    }
                }
                catch (System.Exception e)
                {
                    ShowError(e);
                }            
            };

            Field.AfterTextChanged += (sender, arguments) =>
            {
                try
                {
                    if (count > 0)
                    {
                        switch (Field.Text[start])
                        {
                            case '\n':
                            {
                                OnLineEnding(start, (start == 0 ? false : (Field.Text[start - 1] == '{')));
                                break;
                            }
                            case '{':
                            {
                                OnBrackets(start);
                                break;
                            }
                            case '[':
                            {
                                OnArrayCloses(start);
                                break;
                            }
                            case '(':
                            {
                                OnCloses(start);
                                break;
                            }
                        }
                    }

                    var end = Math.Min(Field.Text.Length, start + count + LongestHighlight);
                    start = Math.Max(start - LongestHighlight, 0);

                    foreach (var highlight in Highlights)
                    {
                        int i = start;

                        while ((i = Field.Text.IndexOf(highlight.Text, i, end - i)) != -1)
                        {
                            if (!isHighlightLegal(i, highlight.Text.Length))
                            {
                                i += highlight.Text.Length;
                                continue;
                            }

                            arguments.Editable.SetSpan(new ForegroundColorSpan(highlight.Color), i, (i += highlight.Text.Length), SpanTypes.PointPoint);
                        }
                    }

                    PostProcess();
                }
                catch (System.Exception e)
                {
                    ShowError(e);
                }
            };

            Find<ImageView>(Resource.Id.Editor_Options).Click += OnEditorOptions;

            try
            {
                var files = new DirectoryInfo(Root).GetFiles("*.cs");

                if (files.Length > 0)
                {
                    Open(files[0].Name);
                }
                else
                {
                    File.WriteAllText(Root + "/Main.cs", CommandStartFile);
                    Open("Main.cs");
                }
            }
            catch (Exception e)
            {
                ShowError(e);
            }        
        }

        private void PostProcess ()
        {
            var i = 0;

            while ((i = Field.Text.IndexOf("//", i)) != -1)
            {
                var l = Field.Text.IndexOf('\n', i);

                if (l != -1)
                {
                    Field.EditableText.SetSpan(new ForegroundColorSpan(CommentHighlightColor), i, l, SpanTypes.PointPoint);
                }

                i += 2;
            }

            i = 0;
            
            while ((i = Field.Text.IndexOf('"', i)) != -1)
            {
                var l = Field.Text.IndexOf('"', i + 1);
                var j = Field.Text.IndexOf('\n', i);

                if (l != -1)
                {
                    if (j == -1 || j > l)
                    {
                        Field.EditableText.SetSpan(new ForegroundColorSpan(TextHighlightColor), i, ++l, SpanTypes.PointPoint);
                    }

                    i = l;
                }
                else
                {
                    break;
                }
            }

            i = 0;

            while ((i = Field.Text.IndexOf("'", i)) != -1)
            {
                var l = Field.Text.IndexOf("'", i + 1);
                var j = Field.Text.IndexOf("\n", i);

                if (l != -1)
                {
                    if (j == -1 || j > l)
                    {
                        Field.EditableText.SetSpan(new ForegroundColorSpan(TextHighlightColor), i, ++l, SpanTypes.PointPoint);
                    }
                   
                    i = l;
                }
                else
                {
                    break;
                }              
            }
        }

        private bool isHighlightLegal (int start, int length)
        {
            return (start == 0 || isSymbol(Field.Text[start - 1])) &&
                   (start + length == Field.Text.Length || isSymbol(Field.Text[start + length]));
        }

        private char[] Symbols = " 0123456789!%&/{([)]=}?+^*><|;,:.\n".ToArray();

        private bool isSymbol (char c)
        {
            foreach (char symbol in Symbols)
            {
                if (c == symbol)
                {
                    return true;
                }
            }

            return false;
        }

        private void Save (System.Action onFinished)
        {
            try
            {
                File.WriteAllText(Root + "/" + Name, Field.Text);
            }
            catch (System.Exception e)
            {
                ShowError(e);
            }

            onFinished?.Invoke();
        }

        private void Open (string name)
        {
            try
            {
                Field.Text = File.ReadAllText(Root + "/" + (this.Name = name));
            }
            catch (System.Exception e)
            {
                ShowError(e);
            }       
        }

        private int LastIndexOf (string text, char value, int offset, int count)
        {
            var i = -1;

            while (count-- > 0)
            {
                if (text[offset] == value)
                {
                    i = offset;
                }

                offset++;
            }

            return i;
        }

        private string GetSpaces (int length)
        {
            var buffer = new char[length];

            for (int i = 0; i < length; i++)
            {
                buffer[i] = ' ';
            }

            return new string(buffer);
        }

        private void OnLineEnding (int i, bool tab = false)
        {
            var count = CalculateSpaces(Field.Text, i) + (tab ? 3 : 0);

            Field.Text = Field.Text.Insert(++i, GetSpaces(count));
            Field.SetSelection(i + count);
        }

        private int CalculateSpaces (string text, int start)
        {
            var i = LastIndexOf(text, '\n', 0, start) + 1;
    
            for (int l = i; l < text.Length; l++)
            {
                if (text[l] != ' ')
                {
                    return l - i;
                }
            }

            return 0;
        }

        /*
         * 
         *  Brackets
         * 
         */ 

        private void OnBrackets (int i)
        {
            try
            {
                var text = Field.Text;
                var count = CalculateSpaces(text, i);

                text = text.Insert(i++, "\n");
                text = text.Insert(i, GetSpaces(count));
                i += count + 1;

                text = text.Insert(i++, "\n");
                text = text.Insert(i, GetSpaces(count));
                text = text.Insert(i, GetSpaces(3));

                int l = (i += count + 3);

                text = text.Insert(i++, "\n");

                text = text.Insert(i, GetSpaces(count));
                i += count;

                text = text.Insert(i, "}");

                Field.Text = text;
                Field.SetSelection(l);
            }
            catch (System.Exception e)
            {
                ShowError(e);
            } 
        }

        private void OnArrayCloses (int i)
        {
            Field.Text = Field.Text.Insert(++i, "]");
            Field.SetSelection(i);
        }

        private void OnCloses (int i)
        {
            Field.Text = Field.Text.Insert(++i, ")");
            Field.SetSelection(i);
        }

        /*
         * 
         *  Start
         * 
         */ 

        private void OnEditorOptions (object sender, System.EventArgs e)
        {
            var builder = new AlertDialog.Builder(GetContext());
            
            builder.SetItems(new string[] { "Tiedostot", "Rakenna", "Sulje" }, (list, arguments) =>
            {
                switch (arguments.Which)
                {
                    case 0:
                    {
                        Save(OnProjectFiles);
                        break;
                    }
                    case 1:
                    {
                        Save(OnCompile);
                        break;
                    }
                    case 2:
                    {
                        Save(OnClose);
                        break;
                    }
                }
            });

            builder.Show();
        }

        private void OnCreateFile ()
        {
            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Luo tiedosto");
            builder.SetView(Resource.Layout.File);
            
            builder.SetPositiveButton("OK", (sender, arguments) =>
            {
                var name = ((EditText)((AlertDialog)sender).FindViewById(Resource.Id.File_Name)).Text;

                if (string.IsNullOrEmpty(name))
                {
                    ShowToast("Tiedostonimi ei voi olla tyhjä!");
                    OnCreateFile();
                    return;
                }

                Save(() =>
                {
                    try
                    {
                        if (name.Length >= 3 && name.Substring(name.Length - 3) == ".cs")
                        {
                            using (StreamWriter Writer = File.CreateText(Root + "/" + name))
                            {
                                Writer.Write("using XSC;");
                            }
                        }
                        else
                        {
                            File.Create(Root + "/" + name);
                        }

                        Open(name);
                    }
                    catch (System.Exception e)
                    {
                        ShowError(e);
                    }
                });
            });

            builder.SetNegativeButton("Peruuta", (sender, arguments) => {});
            builder.Show();
        }

        private void OnDeleteFile (string name, AlertDialog dialog)
        {
            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Poista");
            builder.SetMessage("Haluatko varmasti poistaa tämän tiedoston?");

            builder.SetPositiveButton("Kyllä", (Sender, Arguments) =>
            {
                dialog.Dismiss();

                try
                {
                    File.Delete(Root + "/" + name);
                }
                catch (System.Exception e)
                {
                    ShowError(e);
                }
            });

            builder.SetNegativeButton("Ei", (Sender, Arguments) => {});
            builder.Show();
        }

        private void OnProjectFiles ()
        {
            var items = new DirectoryInfo(Root).GetFiles();

            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Tiedostot");
            builder.SetView(Resource.Layout.Selection);

            builder.SetPositiveButton("Luo", (Sender, Arguments) => { OnCreateFile(); });
            builder.SetNegativeButton("Peruuta", (Sender, Arguments) => { });

            var dialog = builder.Show();

            var list = (ListView)dialog.FindViewById(Resource.Id.Selection_List);
            list.Adapter = new ArrayAdapter(GetContext(), Android.Resource.Layout.SimpleListItem1, items.Select(File => File.Name).ToArray());

            list.ItemClick += (Sender, Arguments) =>
            {
                dialog.Dismiss();
                Open(items[Arguments.Position].Name);
            };

            list.ItemLongClick += (Sender, Arguments) =>
            {
                OnDeleteFile(items[Arguments.Position].Name, dialog);
            };
        }

        private void OnCompile()
        {
            var builder = new AlertDialog.Builder(GetContext());
            builder.SetTitle("Rakenna");
            builder.SetView(Resource.Layout.Compile);
            
            builder.SetPositiveButton("OK", (sender, arguments) =>
            {
                try
                {
                    var dialog = (AlertDialog)sender;

                    var request = new CreateRequest();
                    request.Description = ((EditText)dialog.FindViewById(Resource.Id.Compile_Description)).Text;

                    if (string.IsNullOrEmpty((request.Name = ((EditText)dialog.FindViewById(Resource.Id.Compile_Name)).Text)))
                    {
                        ShowToast("Kaikki kentät täytyy olla täytettyinä!");
                        return;
                    }

                    request.Files = new DirectoryInfo(Root).GetFiles().Select(ServerFile => new VirtualFile(ServerFile.Name, File.ReadAllBytes(ServerFile.FullName))).ToArray();
               
                    Connection.Callback = (type, bytes) =>
                    {
                        switch (type)
                        {
                            case MessageType.Succeeded:
                            {
                                CommandActivity.Commands.Add(new CommandInfo(request.Name, request.Description));
                                ShowDialog(Encoding.UTF8.GetString(bytes), "Tulos");
                                break;
                            }
                            case MessageType.Failed:
                            {
                                ShowDialog(Encoding.UTF8.GetString(bytes), "Virhe");
                                break;
                            }
                            default:
                            {
                                ShowToast("Palvelimella tapahtui virhe!");
                                break;
                            }
                        }
                    };

                    Connection.Send(MessageType.Create, request);
                }
                catch (System.Exception e)
                {
                    ShowError(e);
                }   
            });

            builder.SetNegativeButton("Peruuta", (Sender, Arguments) => {});
            builder.Show();
        }

        private void OnClose ()
        {
            new CommandActivity(GetRoot());
        }
    }
}