using System;
using System.Windows;
using System.Windows.Controls;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views
{
    public partial class MainWindow : Window
    {
        private SideMenuManager sideMenuManager;

        public MainWindow()
        {
            InitializeComponent();

            string userRole = GetCurrentUserRole();
            SideMenuManager.CurrentUserRole = userRole;

            NavigationManager.Initialize(MainFrame, NavigationPanel, BtnBack);
            sideMenuManager = new SideMenuManager(NavigationManager.Instance);

            sideMenuManager.LoadButtons(MenuStackPanel);

            BtnProfile.Content = "Jafeth Delgado";

            NavigateToPage("Glb_Principal", new PrincipalPage());

            LoadProfileImage();

            BtnBack.Click += BtnBack_Click;
        }

        public void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }

        private string GetCurrentUserRole()
        {
            return "Waiter";
        }

        private void LoadProfileImage()
        {
            BtnProfile.Tag = new System.Windows.Media.Imaging.BitmapImage(new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
        }

        public void NavigateToPage(string pageName, Page pageInstance)
        {
            NavigationManager.Instance.NavigateToPage(pageName, pageInstance);
        }
    }
}
