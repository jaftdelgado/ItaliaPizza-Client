using System;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para SuppliesPage.xaml
    /// </summary>
    public partial class SuppliesPage : Page
    {
        public SuppliesPage()
        {
            InitializeComponent();
        }

        private void Click_BtnNewSupply(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegSupply_Header", new RegisterSupplyPage());
        }
    }
}
