using Xamarin.Forms;

namespace ScreenRecorder
{
    public partial class ScreenRecorderPage : ContentPage
    {
        IScreenRecorder screenRecorder;

        public ScreenRecorderPage()
        {
            InitializeComponent();
            screenRecorder = DependencyService.Get<IScreenRecorder>();
            screenRecorder.PromptDiscard = true;
            screenRecorder.DiscardMessage = "丢弃删除已经录制视频，预览进一步确认视频内容";
            screenRecorder.DiscardTitle = "是否保留";
            screenRecorder.RecordStart += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("!!RecordStart!!");
            };
            screenRecorder.RecordCompleted += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("!!RecordCompleted!!");
            };
        }

        void Start(object sender, System.EventArgs e)
        {
            screenRecorder.Start(false);
        }

        void Stop(object sender, System.EventArgs e)
        {
            screenRecorder.Stop();
        }
    }
}
