using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItaliaPizzaClient.Utilities
{
    public static class InputUtilities
    {
        public static void ValidateInput(TextBox textBox, string pattern, int maxLength)
        {
            textBox.TextChanged += (s, e) =>
            {
                string input = textBox.Text;
                string cleaned = Regex.Replace(input, pattern, "");

                if (cleaned.Length > maxLength)
                    cleaned = cleaned.Substring(0, maxLength);

                if (input != cleaned)
                {
                    textBox.Text = cleaned;
                    textBox.SelectionStart = cleaned.Length;
                    Animations.ShakeTextBox(textBox);
                }
            };
        }
        
        public static void ValidateInput2(TextBox textBox, string pattern, int maxLength)
        {
            string input = textBox.Text;
            string cleaned = Regex.Replace(input, pattern, "");

            if (cleaned.Length > maxLength)
                cleaned = cleaned.Substring(0, maxLength);

            if (input != cleaned)
            {
                textBox.Text = cleaned;
                textBox.SelectionStart = cleaned.Length;
                Animations.ShakeTextBox(textBox);
            }
        }

        public static void ValidatePasswordInput(PasswordBox passwordBox, string pattern, int maxLength)
        {
            passwordBox.PasswordChanged += (s, e) =>
            {
                string input = passwordBox.Password;
                string cleaned = Regex.Replace(input, pattern, "");

                if (cleaned.Length > maxLength)
                    cleaned = cleaned.Substring(0, maxLength);

                if (input != cleaned)
                {
                    passwordBox.Password = cleaned;
                    Animations.ShakePasswordBox(passwordBox);
                }
            };
        }

        public static void ValidateMonetaryInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.]");
            e.Handled = regex.IsMatch(e.Text);

            if (((TextBox)sender).Text.Contains(".") && e.Text == ".")
                e.Handled = true;
        }

        public static void FormatMonetaryValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                textBox.Text = "0.00";
                return;
            }

            if (!text.Contains("."))
                textBox.Text = $"{text}.00";
            else
            {
                var parts = text.Split('.');
                if (parts.Length == 2)
                    textBox.Text = $"{parts[0]}.{parts[1].PadRight(2, '0').Substring(0, 2)}";
            }
        }

        public static void ConvertToUpperCase(TextBox textBox)
        {
            textBox.TextChanged += (s, e) =>
            {
                textBox.Text = textBox.Text.ToUpper();
                textBox.SelectionStart = textBox.Text.Length;
            };
        }
        
    }
}
