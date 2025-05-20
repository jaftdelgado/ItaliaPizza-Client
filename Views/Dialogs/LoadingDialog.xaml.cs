using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ItaliaPizzaClient.Views.Dialogs
{
    public partial class LoadingDialog : UserControl
    {
        private static LoadingDialog _instance;

        public LoadingDialog()
        {
            InitializeComponent();
        }

        public static void Show()
        {
            Window activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            if (_instance == null)
                _instance = new LoadingDialog();

            var dialogHost = activeWindow.FindName("DialogHost") as ContentControl;
            var dialogOverlay = activeWindow.FindName("DialogOverlay") as Border;

            if (dialogHost == null || dialogOverlay == null) return;

            dialogHost.Content = _instance;
            dialogOverlay.Visibility = Visibility.Visible;

            StartLoadingAnimation();
        }

        public static void Close()
        {
            Window activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            var dialogHost = activeWindow.FindName("DialogHost") as ContentControl;
            var dialogOverlay = activeWindow.FindName("DialogOverlay") as Border;

            if (dialogHost == null || dialogOverlay == null) return;

            dialogHost.Content = null;
            dialogOverlay.Visibility = Visibility.Collapsed;

            _instance = null;
        }

        public static bool IsDialogVisible()
        {
            foreach (var window in Application.Current.Windows.OfType<Window>())
            {
                var dialogHost = window.FindName("DialogHost") as ContentControl;
                if (dialogHost?.Content is LoadingDialog)
                    return true;
            }

            return false;
        }

        private static void StartLoadingAnimation()
        {
            if (_instance == null) return;

            Storyboard rotateStoryboard = (Storyboard)Application.Current.TryFindResource("RotateAnimation");
            rotateStoryboard?.Begin(_instance.LoadIcon, true);
        }

        private static Window GetActiveWindow()
        {
            return Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);
        }
    }
}
