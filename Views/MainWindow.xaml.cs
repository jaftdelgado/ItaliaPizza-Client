using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;

namespace ItaliaPizzaClient.Views
{
    public partial class MainWindow : Window
    {
        private SideMenuManager sideMenuManager;

        public MainWindow()
        {
            InitializeComponent();
            LoginGrid.Visibility = Visibility.Collapsed;
            RootGrid.Visibility = Visibility.Visible;

            SideMenuManager.CurrentUserRole = GetCurrentUserRole();

            NavigationManager.Initialize(MainFrame, NavigationPanel, BtnBack);
            sideMenuManager = new SideMenuManager(NavigationManager.Instance);

            sideMenuManager.LoadButtons(MenuStackPanel);

            //BtnProfile.Content = CurrentSession.Name;

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
            BtnProfile.Tag = new System.Windows.Media.Imaging.BitmapImage(new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
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


        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = txtUsuario.Text;
            var password = txtPassword.Password;
            var hashedPassword = PasswordUtilities.HashPassword(password);
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Error: Campos vacios");
                MessageDialog.Show("Error", "Por favor ingrese su usuario y contraseña",AlertType.ERROR);
                return;
            }
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;

            var personal = client.Login(user, hashedPassword);
            if (personal == null)
            {
                MessageDialog.Show("Error", "Credenciales incorrectas", AlertType.ERROR);
                return;
            }

            
            //...CREAR SINGLETON SESION
            CurrentSession.UserID = personal.PersonalID;
            CurrentSession.Name = personal.FirstName;
            CurrentSession.Surnames = personal.LastName;
            CurrentSession.UserName = personal.Username;
            CurrentSession.UserRole = personal.RoleID;

            SessionManager.Start();
            //...crear metodo ping para actualizar sesion


            //Ocultar componentes para mostrar homw
            LoginGrid.Visibility = Visibility.Collapsed;
            RootGrid.Visibility = Visibility.Visible;
            //Cargar el menu lateral
            LoadLateralMenu();
        }
        private void LoadLateralMenu()
        {
            string userRole = EmployeeRole.GetDefaultEmployeeRoles().Find(r => r.Id == CurrentSession.UserRole)?.Name;
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
