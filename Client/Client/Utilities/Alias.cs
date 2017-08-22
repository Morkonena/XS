using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System;

using XC.Activities;

namespace XC.Utilities
{
    class Alias
    {
        private RootActivity Root;

        public Alias (RootActivity root)
        {
            Root = root;
        }

        public void SetContentView (int id)
        {
            Root.SetContentView(id);
        }

        public T Find<T> (int id) where T: View
        {
            return Root.FindViewById<T>(id);
        }

        public RootActivity GetRoot ()
        {
            return Root;
        }

        public Context GetContext ()
        {
            return Root;
        }

        public void RunOnUiThread (Action action)
        {
            Root.RunOnUiThread(action);
        }

        public void ShowDialog (string text, string title, bool cancelable = false)
        {
            Root.ShowDialog(text, title, cancelable);
        }

        public void ShowError (Exception e)
        {
            Root.ShowDialog(e.ToString(), "Virhe", false);
        }

        public void ShowToast (string text)
        {
            Root.ShowToast(text);
        }
    }
}