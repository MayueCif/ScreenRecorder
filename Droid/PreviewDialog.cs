
using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using Xamarin.Forms;

namespace ScreenRecorder.Droid
{
    public class PreviewDialog : DialogFragment
    {
        private string path;

        public PreviewDialog(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("path is null or empty");
            }
            this.path = path;
        }

        public string Title
        {
            get;
            set;
        }

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //设置全屏
            SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeLightNoTitleBar);
            //SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeNoTitleBarFullScreen);
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {

            var view = inflater.Inflate(Resource.Layout.preview_dialog, container);
            //var toolbar = view.FindViewById<Toolbar>(Resource.Id.id_toolbar);

            var toolbarCancel = view.FindViewById<TextView>(Resource.Id.toolbar_cancel);
            toolbarCancel.Click += (sender, e) =>
            {
                Dismiss();
                File.Delete(path);
            };
            var toolbarSave = view.FindViewById<TextView>(Resource.Id.toolbar_save);
            toolbarSave.Click += (sender, e) =>
            {
                Dismiss();

                //// Save the name and description of a video in a ContentValues map.
                //final ContentValues values = new ContentValues(2);
                //values.put(MediaStore.Video.Media.MIME_TYPE, "image/jpeg");
                //values.put(MediaStore.Video.Media.DATA, filename);
                //// Add a new record (identified by uri)
                //final Uri uri = getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
                //		values);
                //sendBroadcast(new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE,
                //Uri.parse("file://" + filename)));

                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Android.Net.Uri.Parse(path));
                (Forms.Context as Activity).SendBroadcast(mediaScanIntent);
            };
            var toolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbar_title);
            if (!string.IsNullOrEmpty(Title))
            {
                toolbarTitle.Text = Title;
            }
            var videoview = view.FindViewById<VideoView>(Resource.Id.id_videoview);
            videoview.SetMediaController(new MediaController(Forms.Context));
            videoview.SetVideoURI(Android.Net.Uri.Parse(path));
            videoview.RequestFocus();
            videoview.Start();
            return view;
        }
    }

}
