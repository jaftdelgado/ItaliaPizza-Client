using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.Views.RecipesModule;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterProduct : Page
    {
        private byte[] _selectedImageBytes;
        private readonly RecipeDTO _recipe = new RecipeDTO();
        private readonly List<RecipeSupplyDTO> _recipeSupplyDTOs = new List<RecipeSupplyDTO>();

        public RegisterProduct()
        {
            InitializeComponent();
        }

        private void CreateProduct()
        {
            if (!ValidatePriceInput(out decimal price))
            {
                return;
            }

            var product = CreateProductDto(price);
            MainManagerClient serviceClient = null;

            try
            {
                serviceClient = new MainManagerClient();
                var result = ProcessProductCreation(product, serviceClient);

                HandleCreationResult(result);
            }
            catch (TimeoutException ex)
            {
                HandleServiceException(serviceClient, "Timeout", ex);
            }
            catch (CommunicationException ex)
            {
                HandleServiceException(serviceClient, "Error de comunicación", ex);
            }
            catch (Exception ex)
            {
                HandleServiceException(serviceClient, "Error inesperado", ex);
            }
            finally
            {
                CloseServiceClient(serviceClient);
            }
        }

        private bool ValidatePriceInput(out decimal price)
        {
            if (!decimal.TryParse(txt_price.Text, out price))
            {
                MessageDialog.Show("Error", "Por favor ingrese un precio válido.", AlertType.ERROR);
                return false;
            }
            return true;
        }

        private ProductDTO CreateProductDto(decimal price)
        {
            return new ProductDTO
            {
                Name = txt_name.Text,
                Description = txt_description.Text,
                Price = price,
                Category = CbCategories.Text,
                IsPrepared = (bool)CbPrepared.IsChecked,
                Code = txt_code.Text,
                Photo = _selectedImageBytes
            };
        }

        private ProductDTO ProcessProductCreation(ProductDTO product, MainManagerClient client)
        {
            ProductDTO result = client.AddProduct(product);

            if (product.IsPrepared && result != null)
            {
                _recipe.ProductID = result.Id;
                var recipeResult = client.RegisterRecipe(_recipe, _recipeSupplyDTOs.ToArray());

                if (recipeResult <= 0)
                {
                    MessageDialog.Show("Error", "El producto se creó pero hubo un problema al registrar la receta.", AlertType.WARNING);
                    return null;
                }
            }

            return result;
        }

        private void HandleCreationResult(ProductDTO result)
        {
            if (result != null)
            {
                MessageDialog.Show("Éxito", "Producto creado correctamente", AlertType.SUCCESS);
                NavigationService?.GoBack();
            }
            else
            {
                MessageDialog.Show("Error", "No se pudo crear el producto", AlertType.ERROR);
            }
        }

        private void HandleServiceException(MainManagerClient client, string errorType, Exception ex)
        {
            client?.Abort();
            Console.WriteLine($"Error: {ex.Message}");
            MessageDialog.Show(errorType, $"Ocurrió un error: {ex.Message}", AlertType.ERROR);
        }

        private void CloseServiceClient(MainManagerClient client)
        {
            if (client?.State == CommunicationState.Faulted)
            {
                client.Abort();
            }
            else
            {
                client?.Close();
            }
        }

        private bool ValidateInputs()
        {
            if (_selectedImageBytes == null || _selectedImageBytes.Length == 0 ||
                string.IsNullOrWhiteSpace(txt_name.Text) ||
                string.IsNullOrWhiteSpace(txt_description.Text) ||
                string.IsNullOrWhiteSpace(txt_price.Text) ||
                string.IsNullOrWhiteSpace(CbCategories.Text))
            {
                MessageDialog.Show("Error", "Por favor, complete todos los campos.", AlertType.WARNING);
                return false;
            }

            if (!decimal.TryParse(txt_price.Text, out _))
            {
                MessageDialog.Show("Error", "Por favor ingrese un precio válido.", AlertType.WARNING);
                return false;
            }

            return true;
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
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
                    MessageDialog.Show("Error", $"Error al cargar la imagen: {ex.Message}", AlertType.ERROR);
                }
            }
        }

        private void BtnNewProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                CreateProduct();
            }
        }

        private void PreviewTextInput_TbUnitPrice(object sender, TextCompositionEventArgs e)
        {
            InputUtilities.ValidateMonetaryInput(sender, e);
        }

        private void Click_BtnAddRecipe(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToPage("RegProduct_Header", new RecipeRegister(_recipe, _recipeSupplyDTOs));
            }
        }

        private void CbPrepared_Checked(object sender, RoutedEventArgs e)
        {
            BtnAddRecipe.Visibility = Visibility.Visible;
            txt_code.IsEnabled = false;
        }

        private void CbPrepared_Unchecked(object sender, RoutedEventArgs e)
        {
            BtnAddRecipe.Visibility = Visibility.Collapsed;
            txt_code.IsEnabled = true;
        }
    }
}