using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XC.Utilities
{
    class Selection
    {
        private class Item
        {
            public string Name;
            public bool isFolder;

            public Item (string name, bool isDirectory)
            {
                Name = name;
                isFolder = isDirectory;
            }
        }

        private static List<Item> GetItems(string folder)
        {
            var items = new List<Item>();
            items.Add(new Item("...", true));
            
            try
            {
                DirectoryInfo information = new DirectoryInfo(folder);

                items.AddRange(information.GetFiles().Select(i => new Item(i.Name, false)));
                items.AddRange(information.GetDirectories().Select(i => new Item(i.Name, true)));
            }
            catch {}

            return items;
        }

        public static string InternalStorage = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

        public static void SelectFolder (Context context, string path, Action<string> onComplete)
        {
            var items = GetItems(path);

            var builder = new AlertDialog.Builder(context);
            builder.SetTitle("Valitse kansio");
            builder.SetCancelable(false);
            builder.SetView(Resource.Layout.Selection);

            builder.SetPositiveButton("OK", (sender, arguments) => onComplete(path));
            builder.SetNegativeButton("Peruuta", (sender, arguments) => onComplete(null));

            var dialog = builder.Show();

            var list = (ListView)dialog.FindViewById(Resource.Id.Selection_List);
            list.Adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleListItem1, items.Select(item => item.Name).ToArray());

            list.ItemClick += (Sender, Arguments) =>
            {
                if (Arguments.Position == 0)
                {
                    if (path != InternalStorage)
                    {
                        path = path.Substring(0, path.LastIndexOf('/'));
                    }
                }
                else if (items[Arguments.Position].isFolder)
                {
                    path += "/" + items[Arguments.Position].Name;
                }

                if (items[Arguments.Position].isFolder)
                {
                    items = GetItems(path);
                    list.Adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleListItem1, items.Select(item => item.Name).ToArray());
                }
            };
        }

        /*public static void SelectFile(Context context, string path, string title, Action<string> onComplete)
        {
            List<Item> items = GetItems(path);
            ListView list = null;

            var builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetView(Resource.Layout.SelectionDialog);

            builder.SetPositiveButton("OK", (sender, arguments) =>
            {
                onComplete(path + "\\" + items[list.SelectedItemPosition].Name);
            });

            builder.SetNegativeButton("Peruuta", (sender, arguments) =>
            {
                onComplete(null);
            });

            var dialog = builder.Show();

            list = (ListView)dialog.FindViewById(Resource.Id.SelectionList);
            list.Adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleListItem1, items.Select(item => item.Name).ToArray());

            list.ItemClick += (sender, arguments) =>
            {
                if (arguments.Position == 0)
                {
                    if (path != InternalStorage)
                    {
                        path = path.Substring(0, path.LastIndexOf('/'));
                    }
                }
                else if (items[arguments.Position].isFolder)
                {
                    path += "/" + items[arguments.Position].Name;
                }
                else
                {
                    list.SetSelection(arguments.Position);
                   
                    dialog.GetButton((int)DialogButtonType.Positive).Enabled = true;
                    return;
                }

                if (items[arguments.Position].isFolder)
                {          
                    dialog.GetButton((int)DialogButtonType.Positive).Enabled = false;

                    items = GetItems(path);
                    list.Adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleListItem1, items.Select(item => item.Name).ToArray());
                }
            };
        }*/
    }
}