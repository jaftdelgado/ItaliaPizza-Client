using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
            UpdateButtonState();
            TbUsername.TextChanged += (s, e) => UpdateButtonState();
            TbPassword.TextChanged += (s, e) => UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            BtnSignIn.IsEnabled = !string.IsNullOrWhiteSpace(TbUsername.Text) &&
                                  !string.IsNullOrWhiteSpace(TbPassword.Text);
        }

        private void NavigateToMainWindow()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private async Task Login()
        {
            var user = TbUsername.Text;
            var password = TbPassword.Text;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var hashedPassword = PasswordUtilities.HashPassword(password);

                var personal = client.Login(user, hashedPassword);

                if (personal == null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageDialog.Show("Error", "Credenciales incorrectas", AlertType.ERROR);
                    });
                    return;
                }

                CurrentSession.UserID = personal.PersonalID;
                CurrentSession.Name = personal.FirstName;
                CurrentSession.Surnames = personal.LastName;
                CurrentSession.UserName = personal.Username;
                CurrentSession.UserRole = personal.RoleID;

                SessionManager.Start();

                await Application.Current.Dispatcher.InvokeAsync(() => NavigateToMainWindow());
            });
        }


        private async void Click_BtnSignIn(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private void Password_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && PbPassword.Password != textBox.Text)
                PbPassword.Password = textBox.Text;
            else if (sender is PasswordBox passwordBox && TbPassword.Text != passwordBox.Password)
                TbPassword.Text = passwordBox.Password;
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.ShowPassword(TbPassword, PbPassword);
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.HidePassword(TbPassword, PbPassword);
        }
    }
}
