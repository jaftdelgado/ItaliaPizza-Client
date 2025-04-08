using System;
using System.Collections.Generic;
using ItaliaPizzaClient.ItaliaPizzaServices;
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
using System.Collections.ObjectModel;
using System.Globalization;
using ItaliaPizzaClient.Model;

namespace ItaliaPizzaClient.Views.RecipesModule
{
    /// <summary>
    /// Interaction logic for RecipesRegister.xaml
    /// </summary>
    public partial class RecipeRegister : Page
    {
       public ObservableCollection<RecipeSupplyItem> suppliesList { get; } = new ObservableCollection<RecipeSupplyItem>();
        
        public RecipeRegister()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BtnNewRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void suppliesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Click_BtnAddSupply(object sender, RoutedEventArgs e)
        {
           SuppliesList suppliesListPage = new SuppliesList(suppliesList);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            mainWindow.DialogHost.Content = suppliesListPage;
            mainWindow.DialogOverlay.Visibility = Visibility.Visible;



        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Permite solo números, punto decimal y signo negativo
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            e.Handled = !double.TryParse(newText, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }
    }
}
