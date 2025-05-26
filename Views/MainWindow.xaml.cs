using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.UserControls;

namespace ItaliaPizzaClient.Views
{
    public partial class MainWindow : Window
    {
        private SideMenuManager sideMenuManager;

        public MainWindow()
        {
            InitializeComponent();
            SideMenuManager.CurrentUserRole = GetCurrentUserRole();

            NavigationManager.Initialize(MainFrame, NavigationPanel, BtnBack);
            sideMenuManager = new SideMenuManager(NavigationManager.Instance);

            sideMenuManager.LoadButtons(MenuStackPanel);

            NavigateToPage("Glb_Principal", new PrincipalPage());
            ConfigureUserProfileButton();


            BtnBack.Click += BtnBack_Click;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = this;
        }

        public void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }

        private int GetCurrentUserRole()
        {
            return CurrentSession.LoggedInUser.RoleID;
        }

        private void ConfigureUserProfileButton()
        {
            var user = CurrentSession.LoggedInUser;
            if (user == null) return;

            BtnProfile.ApplyTemplate();

            var profileImage = BtnProfile.Template.FindName("PART_ProfileImage", BtnProfile) as Image;
            var nameBlock = BtnProfile.Template.FindName("PART_UserName", BtnProfile) as TextBlock;
            var roleBlock = BtnProfile.Template.FindName("PART_UserRole", BtnProfile) as TextBlock;

            ImageUtilities.SetImageSource(profileImage, user.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);

            if (nameBlock != null) nameBlock.Text = user.Username;
            if (roleBlock != null) roleBlock.Text = user.TranslatedRole;
        }


        public void NavigateToPage(string pageName, Page pageInstance)
        {
            NavigationManager.Instance.NavigateToPage(pageName, pageInstance);
        }

        private void PopUpOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == PopUpOverlay)
            {
                PopUpOverlay.Visibility = Visibility.Collapsed;
                PopUpHost.Content = null;
            }

            e.Handled = true;
        }

        private void Click_BtnProfile(object sender, RoutedEventArgs e)
        {
            UserSettings.Show(sender as FrameworkElement);
        }
    }
}
