using System;
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
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            if (_instance == null)
                _instance = new LoadingDialog();

            mainWindow.DialogHost.Content = _instance;
            mainWindow.DialogOverlay.Visibility = Visibility.Visible;
            StartLoadingAnimation();
        }

        public static void Close()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            mainWindow.DialogHost.Content = null;
            mainWindow.DialogOverlay.Visibility = Visibility.Collapsed;

            _instance = null;
        }

        private static void StartLoadingAnimation()
        {
            if (_instance == null) return;

            Storyboard rotateStoryboard = (Storyboard)Application.Current.TryFindResource("RotateAnimation");
            rotateStoryboard.Begin(_instance.LoadIcon, true);
        }

    }
}
