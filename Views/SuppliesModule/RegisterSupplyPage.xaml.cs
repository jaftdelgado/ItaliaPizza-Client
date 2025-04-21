using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterSupplyPage.xaml
    /// </summary>
    public partial class RegisterSupplyPage : Page
    {
        private byte[] _selectedImageBytes;

        public RegisterSupplyPage()
        {
            InitializeComponent();
            SetCategoriesComboBox();
            SetInputFields();

            KeepAlive = true;
        }

        private void SetInputFields()
        {
            InputUtilities.ValidateInput(TbSupplyName, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbSupplyBrand, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES);
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen",
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _selectedImageBytes = File.ReadAllBytes(openFileDialog.FileName);

                    ImageUtilities.SetImageSource(SupplyImage, _selectedImageBytes, Constants.DEFAULT_SUPPLY_PIC_PATH);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetCategoriesComboBox()
        {
            var categories = SupplyCategory.GetDefaultSupplyCategories();

            foreach (var category in categories)
            {
                category.Name = Application.Current.Resources[category.ResourceKey]?.ToString() ?? category.Name;
            }

            CbCategories.ItemsSource = categories;
        }

        private void CheckbBrand_Checked(object sender, RoutedEventArgs e)
        {
            TbSupplyBrand.IsEnabled = false;
        }
        
        private void CheckbBrand_Unchecked(object sender, RoutedEventArgs e)
        {
            TbSupplyBrand.IsEnabled = true;
        }

        private void PreviewTextInput_TbUnitPrice(object sender, TextCompositionEventArgs e)
        {
            InputUtilities.ValidateMonetaryInput(sender, e);
        }

        private void LostFocus_TbUnitPrice(object sender, RoutedEventArgs e)
        {
            InputUtilities.FormatMonetaryValue(sender, e);
        }

        private void Click_BtnRegisterSupplier(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegSupplier_Header", new RegisterSupplierPage());
        }

        private void Click_BtnRegisterSupply(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {

        }

        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
        }
    }
}
