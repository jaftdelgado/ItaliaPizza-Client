using System;
using System.Windows;
using System.Windows.Controls;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views
{
    public partial class MainWindow : Window
    {
        private NavigationManager navigationManager;
        private SideMenuManager sideMenuManager; // Nueva instancia de SideMenuManager

        public MainWindow()
        {
            InitializeComponent();

            string userRole = GetCurrentUserRole();
            SideMenuManager.CurrentUserRole = userRole;

            // Primero, crear la instancia de NavigationManager
            navigationManager = new NavigationManager(MainFrame, NavigationPanel, BtnBack);

            // Luego, pasarla al constructor de SideMenuManager
            sideMenuManager = new SideMenuManager(navigationManager);

            sideMenuManager.LoadButtons(MenuStackPanel); // Ahora usamos la instancia

            BtnProfile.Content = "Jafeth Delgado";

            NavigateToPage("Glb_Principal", new PrincipalPage());

            LoadProfileImage();

            BtnBack.Click += BtnBack_Click;
        }


        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            navigationManager.GoBack();
        }

        private string GetCurrentUserRole()
        {
            return "Admin";
        }

        private void LoadProfileImage()
        {
            BtnProfile.Tag = new System.Windows.Media.Imaging.BitmapImage(new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Relative));
        }

        public void NavigateToPage(string pageName, Page pageInstance)
        {
            navigationManager.NavigateToPage(pageName, pageInstance);
        }
    }
}
