using System.IO;
using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.Media;
using Android.Media.Projection;
using AEnvironment = Android.OS.Environment;
using Xamarin.Forms;
using System;
using Android.Util;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Views;

[assembly: Dependency(typeof(ScreenRecorder.Droid.ScreenRecorder))]
namespace ScreenRecorder.Droid
{
    public class ScreenRecorder : IScreenRecorder
    {

        static readonly int REQUEST_RECORD = 2017_1;
        static readonly int REQUEST_PERMISSIONS = 2017_2;

        //管理MediaProjection
        static MediaProjectionManager mediaProjectionManager;
        //提供截取屏幕或者记录系统音频的能力
        static MediaProjection mediaProjection;
        //录制音频和视频的类
        static MediaRecorder mediaRecorder;
        //VirtualDisplay类代表一个虚拟显示器
        static VirtualDisplay virtualDisplay;
        //屏幕密度
        static DisplayMetricsDensity screenDensity;

        static int DISPLAY_WIDTH = 480;
        static int DISPLAY_HEIGHT = 800;

        //当前Activiy，Xamarin.Forms中一个App仅一个Activity，即MainActivity
        Activity mainActivity;

        public bool PromptDiscard
        {
            get;
            set;
        }

        public string DiscardTitle
        {
            get;
            set;
        }

        public string DiscardMessage
        {
            get;
            set;
        }


        public bool IsRunning
        {
            get;
            set;
        }

        public event RecordEventHandler RecordStart;

        public event RecordEventHandler RecordCompleted;


        private string SavePath
        {
            get;
            set;
        } = AEnvironment.GetExternalStoragePublicDirectory(AEnvironment.DirectoryDownloads).Path;

        private string path;

        //readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        //long CurrentTimeMillis()
        //{
        //    return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        //}


        public ScreenRecorder()
        {

            mainActivity = (Forms.Context as Activity);
            //服务第一次创建进行初始化
            mediaProjectionManager = (MediaProjectionManager)Forms.Context.GetSystemService(Context.MediaProjectionService);
            mediaRecorder = new MediaRecorder();

            DisplayMetrics metrics = new DisplayMetrics();
            mainActivity.WindowManager.DefaultDisplay.GetMetrics(metrics);
            screenDensity = metrics.DensityDpi;

        }

        public void Start(bool microphoneEnabled)
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = !IsRunning;

            // 权限判断
            if (ContextCompat.CheckSelfPermission(Forms.Context, Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(mainActivity,
                        new String[] { Manifest.Permission.WriteExternalStorage }, REQUEST_PERMISSIONS);
            }

            if (ContextCompat.CheckSelfPermission(Forms.Context, Manifest.Permission.RecordAudio) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(mainActivity,
                        new String[] { Manifest.Permission.RecordAudio }, REQUEST_PERMISSIONS);
            }

            if (ContextCompat.CheckSelfPermission(Forms.Context, Manifest.Permission.CaptureSecureVideoOutput) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(mainActivity,
                       new String[] { Manifest.Permission.CaptureSecureVideoOutput }, REQUEST_PERMISSIONS);
            }


            var message = "";
            try
            {

                //初始化Recorder
                if (microphoneEnabled)
                {
                    mediaRecorder.SetAudioSource(AudioSource.Mic);
                }
                else
                {
                    mediaRecorder.SetAudioSource(AudioSource.Default);
                }
                mediaRecorder.SetVideoSource(VideoSource.Surface);

                mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
                //视频帧数
                mediaRecorder.SetVideoFrameRate(30);
                mediaRecorder.SetVideoSize(DISPLAY_WIDTH, DISPLAY_HEIGHT);
                mediaRecorder.SetVideoEncodingBitRate(5 * 1024 * 1024);
                path = Path.Combine(SavePath, System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4");
                mediaRecorder.SetOutputFile(path);

                mediaRecorder.SetAudioEncoder(AudioEncoder.AmrNb);
                mediaRecorder.SetVideoEncoder(VideoEncoder.H264);

                mediaRecorder.Prepare();

                Intent captureIntent = mediaProjectionManager.CreateScreenCaptureIntent();
                //Form.Context is same as your MainActivity. You should override your MainActivity.OnActivityResult instead.
                //or  https://martynnw.wordpress.com/2016/12/18/android-startactivityforresult-and-xamarin-forms/
                //or  https://forums.xamarin.com/discussion/81278/how-to-handle-the-result-of-startactivityforresult-in-forms
                mainActivity.StartActivityForResult(captureIntent, REQUEST_RECORD);

                message = "go to ActivityResult method";

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                RecordStart?.Invoke(this, new ErrorEventArgs(message));
            }
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            IsRunning = !IsRunning;

            var message = "";
            try
            {
                mediaRecorder.Stop();
                mediaRecorder.Reset();
                virtualDisplay.Release();
                if (PromptDiscard)
                {
                    var promptDialog = new PromptDialog(DiscardTitle, DiscardMessage);
                    promptDialog.Show(mainActivity.FragmentManager, "PromptDialog");
                    promptDialog.DiscardClick += (sender, e) =>
                    {
                        //删除文件
                        File.Delete(path);
                    };
                    promptDialog.PreviewClick += (sender, e) =>
                    {
                        showPreviewDialog();
                    };
                }
                else
                {
                    showPreviewDialog();
                }
                message = "Completed";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                RecordCompleted?.Invoke(this, new ErrorEventArgs(message));
            }

        }

        private void showPreviewDialog()
        {
            //弹出预览视图
            new PreviewDialog(path).Show(mainActivity.FragmentManager, "PreviewDialog");
        }


        public static void ActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == REQUEST_RECORD)
            {
                if (resultCode == Result.Ok)
                {
                    mediaProjection = mediaProjectionManager.GetMediaProjection((int)resultCode, data);
                    mediaProjection.RegisterCallback(new MediaProjectionCallback(), null);

                    // DisplayManager.VirtualDisplayFlagAutoMirror,
                    virtualDisplay = mediaProjection.CreateVirtualDisplay("MainActivity", DISPLAY_WIDTH, DISPLAY_HEIGHT, (int)screenDensity,
                                                                          DisplayFlags.Presentation, mediaRecorder.Surface, null, null);

                    mediaRecorder.Start();


                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("!!Result Code Is" + resultCode + "!!");
                }

            }
        }
    }


    class MediaProjectionCallback : MediaProjection.Callback
    {
        public override void OnStop()
        {
            base.OnStop();
            System.Diagnostics.Debug.WriteLine("!!MediaProjectionCallback Stop!!");
        }
    }


}
