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
            Application.Current.MainWindow = this;

        }

        public void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }

        private string GetCurrentUserRole()
        {
            return "Test";
        }

        private void LoadProfileImage()
        {
            BtnProfile.Tag = new System.Windows.Media.Imaging.BitmapImage(
                new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
        }

        private void ConfigureUserProfileButton()
        {
            var user = CurrentSession.LoggedInUser;
            if (user == null) return;

            BtnProfile.ApplyTemplate();

            var profileImage = BtnProfile.Template.FindName("PART_ProfileImage", BtnProfile) as Image;
            var nameBlock = BtnProfile.Template.FindName("PART_UserName", BtnProfile) as TextBlock;
            var roleBlock = BtnProfile.Template.FindName("PART_UserRole", BtnProfile) as TextBlock;

            if (profileImage != null)
            {
                if (user.ProfilePic != null && user.ProfilePic.Length > 0)
                {
                    profileImage.Source = ImageUtilities.ConvertToImageSource(user.ProfilePic);
                }
                else
                {
                    profileImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
                }
            }

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
