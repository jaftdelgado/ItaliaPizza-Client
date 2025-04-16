using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ItaliaPizzaClient.Views.Dialogs
{
    public partial class MessageDialog : UserControl
    {
        private Action _onConfirm;
        public static readonly (string Brush, string ButtonText) DangerStyle = ("DangerBrush", "Glb_Delete");

        public MessageDialog()
        {
            InitializeComponent();
        }

        public static void Show(string title, string description, AlertType alertType)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            var dialog = new MessageDialog();

            dialog.DialogTitle.Text = FindResourceString(title);
            dialog.DialogDescription.Text = FindResourceString(description);

            dialog.ConfigureAlertType(alertType);

            mainWindow.DialogHost.Content = dialog;
            mainWindow.DialogOverlay.Visibility = Visibility.Visible;
        }

        public static void ShowConfirm(string title, string description, Action onConfirm,
            string dangerButtonTextResourceKey = null)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            var dialog = new MessageDialog
            {
                _onConfirm = onConfirm
            };

            dialog.DialogTitle.Text = FindResourceString(title);
            dialog.DialogDescription.Text = FindResourceString(description);
            dialog.SetAlertProperties("PrimaryButtonHoverBrush", "confirm-alert-icon.png", "Glb_Accept", Visibility.Visible);
            dialog.BtnClose.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(dangerButtonTextResourceKey))
                dialog.BtnAccept.Content = FindResourceString("Glb_Confirm");
            else
            {
                dialog.BtnAccept.Background = (SolidColorBrush)Application.Current.Resources["DangerBrush"];
                dialog.BtnAccept.Content = FindResourceString(dangerButtonTextResourceKey);
            }

            mainWindow.DialogHost.Content = dialog;
            mainWindow.DialogOverlay.Visibility = Visibility.Visible;
        }

        private static string FindResourceString(string resourceKey)
        {
            var resource = Application.Current.TryFindResource(resourceKey);
            return resource != null ? resource.ToString() : $"{resourceKey}";
        }

        private void ConfigureAlertType(AlertType alertType)
        {
            switch (alertType)
            {
                case AlertType.SUCCESS:
                    SetAlertProperties("SuccessBrush", "success-alert-icon.png", "Glb_Accept", Visibility.Collapsed);
                    break;
                case AlertType.WARNING:
                    SetAlertProperties("WarningBrush", "warning-alert-icon.png", "Glb_TryAgain", Visibility.Collapsed);
                    break;
                case AlertType.ERROR:
                    SetAlertProperties("DangerBrush", "error-alert-icon.png", "Glb_Close", Visibility.Collapsed);
                    break;
            }
        }

        private void SetAlertProperties(string brushResourceKey, string iconSource, string buttonTextResourceKey, Visibility secondaryButtonVisibility)
        {
            var brush = (SolidColorBrush)Application.Current.Resources[brushResourceKey];

            HeaderBorder.Background = brush;
            AlertIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"/Resources/Icons/{iconSource}", UriKind.Relative));
            BtnAccept.Content = FindResourceString(buttonTextResourceKey);
            BtnClose.Visibility = secondaryButtonVisibility;
        }

        private void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            _onConfirm?.Invoke();
            CloseDialog();
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            CloseDialog();
        }

        private void CloseDialog()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.DialogHost.Content = null;
                mainWindow.DialogOverlay.Visibility = Visibility.Collapsed;
            }
        }
    }

    public enum AlertType
    {
        SUCCESS,
        WARNING,
        ERROR
    }
}
