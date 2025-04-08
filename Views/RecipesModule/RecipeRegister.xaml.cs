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
using ItaliaPizzaClient.Views.Dialogs;

namespace ItaliaPizzaClient.Views.RecipesModule
{
    /// <summary>
    /// Interaction logic for RecipesRegister.xaml
    /// </summary>
    public partial class RecipeRegister : Page
    {
        public ObservableCollection<RecipeSupplyItem> suppliesList { get; } = new ObservableCollection<RecipeSupplyItem>();
        private readonly RecipeDTO _recipe;
        private readonly List<RecipeSupplyDTO> _recipeSupplyDTOs;


        public RecipeRegister(RecipeDTO recipe,List<RecipeSupplyDTO> recipeSupplyDTOs)
        {
            InitializeComponent();
            _recipe = recipe;
            _recipeSupplyDTOs = recipeSupplyDTOs;
            DataContext = this;
        }

        private void BtnNewRecipe_Click(object sender, RoutedEventArgs e)
        {
            //Validar entradas
            if (string.IsNullOrEmpty(txt_description.Text) || string.IsNullOrEmpty(txt_description.Text) || isColumsCorrect() || string.IsNullOrEmpty(txt_preptime.Text))
            {
                MessageDialog.Show("Error", "Por favor, complete todos los campos.", AlertType.WARNING);
                return;
            }
            else
            {
                _recipe.Name = txt_description.Text;
                _recipe.Description = txt_description.Text;
                _recipe.PreparationTime = int.Parse(txt_preptime.Text);

                //Item de la receta a RecipeSupplyDTO
                foreach (var item in suppliesList)
                {
                    RecipeSupplyDTO recipeSupplyDTO = new RecipeSupplyDTO
                    {
                        SupplyID = item.Supply.Id,
                         UseQuantity = (decimal)item.Quantity
                    };
                    _recipeSupplyDTOs.Add(recipeSupplyDTO);
                }
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        private bool isColumsCorrect()
        {
            bool valid = false;
            foreach (var item in suppliesList)
            {
                if (item.Quantity <= 0)
                {
                    MessageDialog.Show("Error", "Por favor, complete todos los campos.", AlertType.WARNING);
                    valid = true;
                }
            }
            return valid;
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
