using ItaliaPizzaClient.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class OpeningCash : UserControl
    {
        public OpeningCash()
        {
            InitializeComponent();
        }

        public static void Show(FrameworkElement triggerButton)
        {
            var openingCash = new OpeningCash();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var popupContainer = mainWindow.PopUpHost.Parent as FrameworkElement;

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            openingCash.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = openingCash.DesiredSize.Width;
            double popupHeight = openingCash.DesiredSize.Height;

            double left = mainWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            mainWindow.PopUpHost.Content = openingCash;
            Canvas.SetLeft(mainWindow.PopUpHost, left);
            Canvas.SetTop(mainWindow.PopUpHost, top);

            mainWindow.PopUpOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(openingCash, "ShowBorderAnimation");
        }


        private void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            // lógica de aceptación
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;


            mainWindow.PopUpOverlay.Visibility = Visibility.Collapsed;
            mainWindow.PopUpHost.Content = null;
        }

    }
}
