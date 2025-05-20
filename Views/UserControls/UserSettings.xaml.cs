using ItaliaPizzaClient.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ItaliaPizzaClient.Views.Dialogs;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class UserSettings : UserControl
    {
        public UserSettings()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        public static void Show(FrameworkElement triggerButton)
        {
            var userSettings = new UserSettings();
            var mainWindow = Application.Current.MainWindow as MainWindow;

            Point relativePoint = triggerButton.TransformToAncestor(mainWindow).Transform(new Point(0, 0));

            mainWindow.PopUpHost.Content = userSettings;
            userSettings.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double controlHeight = userSettings.DesiredSize.Height;

            double left = 10;
            double top = relativePoint.Y + triggerButton.ActualHeight - controlHeight - 60; 

            Canvas.SetLeft(mainWindow.PopUpHost, left);
            Canvas.SetTop(mainWindow.PopUpHost, top);

            mainWindow.PopUpOverlay.Visibility = Visibility.Visible;
            Animations.BeginAnimation(userSettings, "ShowBorderFromBottomAnimation");

            mainWindow.PopUpOverlay.MouseLeftButtonDown -= PopUpOverlay_MouseLeftButtonDown;
            mainWindow.PopUpOverlay.MouseLeftButtonDown += PopUpOverlay_MouseLeftButtonDown;
        }


        private void LoadUserInfo()
        {
            var user = CurrentSession.LoggedInUser;
            if (user != null)
            {
                EmployeerName.Text = user.FullName;
                EmployeeRole.Text = user.TranslatedRole;

                ImageUtilities.SetImageSource(EmployeeProfilePic, user.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);
            }
        }

        private static void PopUpOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.PopUpOverlay.Visibility = Visibility.Collapsed;
            mainWindow.PopUpHost.Content = null;

            mainWindow.PopUpOverlay.MouseLeftButtonDown -= PopUpOverlay_MouseLeftButtonDown;
        }

        private void Click_BtnSignOut(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "SignOut_DialogTSignOut",
                "SignOut_DialogDSignOut",
                async () =>
                {

                },
                "Glb_Confirm"
            );
        }
    }
}
