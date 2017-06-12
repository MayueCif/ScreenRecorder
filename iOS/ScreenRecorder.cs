using System;
using System.Diagnostics;
using ReplayKit;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenRecorder.iOS.ScreenRecorder))]
namespace ScreenRecorder.iOS
{
    public class ScreenRecorder : IScreenRecorder
    {
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


        public void Start(bool microphoneEnabled)
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;

            if (!RPScreenRecorder.SharedRecorder.Available)
            {
                Debug.WriteLine("!!不支持支持ReplayKit录制!!");
                return;
            }
            RPScreenRecorder.SharedRecorder.StartRecording(microphoneEnabled, (Foundation.NSError error) =>
            {
                RecordStart?.Invoke(this, new ErrorEventArgs(error?.Description));
            });
        }

        public void Stop()
        {

            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;

            RPScreenRecorder.SharedRecorder.StopRecording((RPPreviewViewController controller, Foundation.NSError error) =>
            {
                RecordCompleted?.Invoke(this, new ErrorEventArgs(error?.Description));

                controller.PreviewControllerDelegate = new MyRPPreviewViewControllerDelegate();

                if (PromptDiscard)
                {
                    var alertController = UIAlertController.Create(DiscardTitle, DiscardMessage, UIAlertControllerStyle.Alert);

                    alertController.AddAction(UIAlertAction.Create("丢弃", UIAlertActionStyle.Cancel, (UIAlertAction obj) =>
                    {
                        RPScreenRecorder.SharedRecorder.DiscardRecording(() =>
                        {
                            Debug.WriteLine("!!丢弃录制内容!!");
                        });
                    }));
                    alertController.AddAction(UIAlertAction.Create("预览", UIAlertActionStyle.Default, (UIAlertAction obj) =>
                    {
                        GetVisibleViewController().PresentViewController(controller, true, null);
                    }));

                    GetVisibleViewController().PresentViewController(alertController, true, null);
                }
                else
                {

                    GetVisibleViewController().PresentViewController(controller, true, null);
                }
            });
        }


        /// 
        /// Gets the visible view controller.
        /// 
        /// The visible view controller.
        UIViewController GetVisibleViewController()
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            if (window == null)
                throw new InvalidOperationException("There's no current active window");

            var rootController = window.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).VisibleViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }

    }

    class MyRPPreviewViewControllerDelegate : RPPreviewViewControllerDelegate
    {
        public override void DidFinish(RPPreviewViewController previewController)
        {
            previewController.DismissViewController(true, null);
        }

        public override void DidFinish(RPPreviewViewController previewController, Foundation.NSSet<Foundation.NSString> activityTypes)
        {

            Debug.WriteLine("!!" + activityTypes + "!!");

            if (activityTypes.Equals("com.apple.UIKit.activity.SaveToCameraRoll"))
            {

            }

        }
    }
}
