using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ItaliaPizzaClient.Views.RecipesModule
{
    /// <summary>
    /// Interaction logic for SuppliesList.xaml
    /// </summary>
    public partial class SuppliesList : UserControl
    {
        List<SupplyDTO> suppliesList = new List<SupplyDTO>();
        public SupplyDTO supplySelected = null;
        private readonly ObservableCollection<RecipeSupplyItem> _listaPrincipal;
        public SuppliesList(System.Collections.ObjectModel.ObservableCollection<RecipeSupplyItem> suppliesListObs)
        {
            InitializeComponent();
            _listaPrincipal = suppliesListObs;
            LoadSupplies();
        }


        private void LoadSupplies()
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;
            var supplies = client.GetAllSupplies();
            if (supplies != null)
            {   
                suppliesList = supplies.ToList();
                suppliesDataGrid.ItemsSource = suppliesList;
            }
            else
            {
                MessageDialog.Show("Error", "No se pudo cargar la lista de insumos", AlertType.ERROR);
            }
        }

        public  void Show()
        {

        }

        private void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            if (suppliesDataGrid.SelectedItem is SupplyDTO selectedSupply)
            {
                var newRecipeItem = new RecipeSupplyItem
                {
                    Supply = selectedSupply,
                    Quantity = 0 
                };
                if(!_listaPrincipal.Any(s => s.Supply.Id == selectedSupply.Id))
                {
                    _listaPrincipal.Add(newRecipeItem);
                }
                else
                {
                    MessageDialog.Show("Advertencia", "El insumo ya está en la lista", AlertType.WARNING);
                }

                // Cerramos el diálogo
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.DialogHost.Content = null;
                    mainWindow.DialogOverlay.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageDialog.Show("Advertencia", "Seleccione un insumo de la lista", AlertType.WARNING);
            }

        }

        private void suppliesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.DialogHost.Content = null;
                mainWindow.DialogOverlay.Visibility = Visibility.Collapsed;
            }
        }
    }
}
