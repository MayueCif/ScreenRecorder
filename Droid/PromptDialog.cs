using System;
using Android.App;
using Android.Views;
using Android.Widget;

namespace ScreenRecorder.Droid
{
    public class PromptDialog : DialogFragment
    {

        public PromptDialog(string title, string message)
        {
            Title = title;
            Message = message;
            Cancelable = false;
        }

        public string Title
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }


        public event EventHandler DiscardClick;

        public event EventHandler PreviewClick;


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {

            Dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
            // Inflate the layout for this fragment
            var view = inflater.Inflate(Resource.Layout.prompt_dialog, container);

            var tv_title = view.FindViewById<TextView>(Resource.Id.tv_title);
            tv_title.Text = Title;
            var tv_message = view.FindViewById<TextView>(Resource.Id.tv_message);
            tv_message.Text = Message;
            var bt_discard = view.FindViewById<Button>(Resource.Id.bt_discard);
            var bt_preview = view.FindViewById<Button>(Resource.Id.bt_preview);
            bt_discard.Click += (sender, e) =>
            {
                Dismiss();
                DiscardClick?.Invoke(sender, e);
            };
            bt_preview.Click += (sender, e) =>
            {
                Dismiss();
                PreviewClick?.Invoke(sender, e);
            };
            return view;
        }

    }
}