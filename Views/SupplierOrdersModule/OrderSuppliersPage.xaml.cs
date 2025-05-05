using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para OrderSuppliersPage.xaml
    /// </summary>
    public partial class OrderSuppliersPage : Page
    {
        public OrderSuppliersPage()
        {
            InitializeComponent();
        }

        private void Clic_BtnNewOrderSupplier(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegOrderSupplier_Header", new RegisterOrderSupplierPage1());

        }

        
    }
}
