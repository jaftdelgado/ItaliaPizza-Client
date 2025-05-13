using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class CashRegisterPage : Page
    {
        public CashRegisterPage()
        {
            InitializeComponent();
        }

        private void Click_BtnOpenRegister(object sender, RoutedEventArgs e)
        {
            UserControls.OpeningCash.Show(sender as FrameworkElement);
        }
    }
}
