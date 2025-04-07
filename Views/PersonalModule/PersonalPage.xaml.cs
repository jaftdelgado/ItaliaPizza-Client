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
    /// Lógica de interacción para PersonalPage.xaml
    /// </summary>
    public partial class PersonalPage : Page
    {
        public PersonalPage()
        {
            InitializeComponent();
        }

        private void Click_BtnNewEmployee(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegEmployee_Header", new RegisterEmployeePage());
        }
    }
}
