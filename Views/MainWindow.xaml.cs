using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views
{
    public partial class MainWindow : Window
    {
        private SideMenuManager sideMenuManager;

        public MainWindow()
        {
            InitializeComponent();
            
            // Inicializar componentes de la ventana principal
            SideMenuManager.CurrentUserRole = GetCurrentUserRole();
            
            NavigationManager.Initialize(MainFrame, NavigationPanel, BtnBack);
            sideMenuManager = new SideMenuManager(NavigationManager.Instance);
            
            sideMenuManager.LoadButtons(MenuStackPanel);
            
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
            return "Test";
        }

        private void LoadProfileImage()
        {
            BtnProfile.Tag = new System.Windows.Media.Imaging.BitmapImage(
                new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
        }

        public void NavigateToPage(string pageName, Page pageInstance)
        {
            NavigationManager.Instance.NavigateToPage(pageName, pageInstance);
        }

        private void PopUpOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == PopUpOverlay)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.PopUpOverlay.Visibility = Visibility.Collapsed;
                mainWindow.PopUpHost.Content = null;
            }

            e.Handled = true;
        }

        private void LoadLateralMenu()
        {
            string userRole = EmployeeRole.GetDefaultEmployeeRoles()
                .Find(r => r.Id == CurrentSession.UserRole)?.Name;
            SideMenuManager.CurrentUserRole = userRole;

            NavigationManager.Initialize(MainFrame, NavigationPanel, BtnBack);
            sideMenuManager = new SideMenuManager(NavigationManager.Instance);

            sideMenuManager.LoadButtons(MenuStackPanel);

            BtnProfile.Content = CurrentSession.Name;

            NavigateToPage("Glb_Principal", new PrincipalPage());

            LoadProfileImage();

            BtnBack.Click += BtnBack_Click;
        }
    }
}