using System;
namespace ScreenRecorder
{
    //定义delegate
    public delegate void RecordEventHandler(object sender, ErrorEventArgs e);

    public interface IScreenRecorder
    {
        /// <summary>
        /// Start the specified microphoneEnabled.
        /// </summary>
        /// <returns>The start.</returns>
        /// <param name="microphoneEnabled">If set to <c>true</c> microphone enabled.</param>
        void Start(bool microphoneEnabled);
        void Stop();
        bool IsRunning
        {
            get;
            set;
        }

        /// <summary>
        /// 是否提示放弃录屏文件
        /// </summary>
        /// <value><c>true</c> if prompt discard; otherwise, <c>false</c>.</value>
        bool PromptDiscard
        {
            get;
            set;
        }

        /// <summary>
        /// 提示标题
        /// </summary>
        /// <value><c>true</c> if discard title; otherwise, <c>false</c>.</value>
        string DiscardTitle
        {
            get;
            set;
        }


        /// <summary>
        /// 提示内容
        /// </summary>
        /// <value><c>true</c> if discard message; otherwise, <c>false</c>.</value>
        string DiscardMessage
        {
            get;
            set;
        }

        //用event 关键字声明事件对象
        event RecordEventHandler RecordStart;

        event RecordEventHandler RecordCompleted;

    }

    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(string error)
        {
            Error = error;
        }

        public string Error
        {
            get;
            set;
        }
    }

}
