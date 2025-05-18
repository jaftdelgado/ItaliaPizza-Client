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
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views.RecipesModule
{
    /// <summary>
    /// Interaction logic for RecipesRegister.xaml
    /// </summary>
    public partial class RecipeRegister : Page
    {
        public ObservableCollection<RecipeSupplyItem> suppliesList { get; } = new ObservableCollection<RecipeSupplyItem>();
        private readonly RecipeDTO _recipe;
        private Recipe _recipeToEdit;
        private readonly List<RecipeSupplyDTO> _recipeSupplyDTOs;
        private bool _isRecipeRegistered;


        public RecipeRegister(RecipeDTO recipe,List<RecipeSupplyDTO> recipeSupplyDTOs)
        {
            InitializeComponent();

            _recipe = recipe;
            _recipeSupplyDTOs = recipeSupplyDTOs;
            DataContext = this;
        }

        public RecipeRegister(Recipe recipeToEdit)
        {
            InitializeComponent();
            _isRecipeRegistered = true;
            _recipeToEdit = recipeToEdit;
            DataContext = this;
            loadSupplies(recipeToEdit.Id);
            loadEditInterface();
            loadEditRecipeData();


        }

        private void loadEditRecipeData()
        {
            txt_description.Text = _recipeToEdit.Description;
            txt_preptime.Text = _recipeToEdit.PreparationTime.ToString();
        }

        private void loadEditInterface()
        {
            txt_description.IsReadOnly = true;
            txt_preptime.IsReadOnly = true;
            colDelete.Visibility = Visibility.Collapsed;
            BtnAddSupply.Visibility = Visibility.Collapsed;
            lblRecipeTitle.Text = "Editar receta";
            BtnNewRecipe.Content = "Actualizar receta";
        }

        private void loadSupplies(int idRecipe)
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;
            var recipeSupplies = client.GetSuppliesByRecipe(idRecipe);
            if (recipeSupplies == null) return;
            foreach (var item in recipeSupplies)
            {
                RecipeSupplyItem recipeSupplyItem = new RecipeSupplyItem
                {
                    Supply = new SupplyDTO
                    {
                        Id = item.SupplyID,
                        Name = item.RecipeSupplyName,
                    },
                    Quantity = (double)item.UseQuantity
                };
                suppliesList.Add(recipeSupplyItem);
            }   
        }

        private void BtnNewRecipe_Click(object sender, RoutedEventArgs e)
        {
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
        private void EliminarFila_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is RecipeSupplyItem item) 
            {
                MessageDialog.ShowConfirm(
                    title: "Confirmar eliminación",
                    description: $"¿Estás seguro que deseas eliminar el insumo '{item.Supply?.Name}'?",
                    onConfirm: () =>
                    {
                        suppliesList.Remove(item);
                    }
                    );
            }
        }
    }
}
