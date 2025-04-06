using System;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Utilities
{
    public static class PasswordUtilities
    {
        public static void ShowPassword(TextBox TbPassword, PasswordBox PbPassword)
        {
            TbPassword.Text = PbPassword.Password;

            PbPassword.Visibility = Visibility.Collapsed;
            TbPassword.Visibility = Visibility.Visible;

            TbPassword.SelectionStart = TbPassword.Text.Length;
            TbPassword.SelectionLength = 0;
            TbPassword.Focus();
        }

        public static void HidePassword(TextBox TbPassword, PasswordBox PbPassword)
        {
            PbPassword.Password = TbPassword.Text;

            TbPassword.Visibility = Visibility.Collapsed;
            PbPassword.Visibility = Visibility.Visible;

            PbPassword.Focus();
        }

        public static void AttachPasswordChangedHandler(PasswordBox PbPassword, Action updateStateCallback)
        {
            PbPassword.PasswordChanged += (sender, e) =>
            {
                updateStateCallback?.Invoke();
            };
        }
    }
}
