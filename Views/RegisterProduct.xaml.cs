using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for RegisterProduct.xaml
    /// </summary>
    public partial class RegisterProduct : Page
    {
        private byte[] _selectedImageBytes;
        public RegisterProduct()
        {
            InitializeComponent();
        }
        private void CreateProduct()
        {
            var product = new ProductDTO
            {
                Name = txt_name.Text,
                Description = txt_description.Text,
                Price = decimal.Parse(txt_price.Text),
                Category = CbCategories.Text.ToString(),
                IsPrepared = (bool)CbPrepared.IsChecked,
                Code = txt_code.Text,
                Photo = _selectedImageBytes
            };
            var serviceClient = new ItaliaPizzaServices.MainManagerClient();
            var result = serviceClient.AddProduct(product);
            serviceClient.Close();
            if (result !=0)
            {
                MessageBox.Show("Producto creado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Error al crear el producto", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //Validar entradas campos vacios
         private bool correctInputs()
        {
            if (_selectedImageBytes == null || _selectedImageBytes.Length == 0 || string.IsNullOrEmpty(txt_name.Text) || string.IsNullOrEmpty(txt_description.Text) || string.IsNullOrEmpty(txt_price.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }else 
                return true;

        }

        //Agregar el boton de registrar receta si es un producto preparado
        //Generar codigo 
        



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

        private void BtnNewProduct_Click(object sender, RoutedEventArgs e)
        {
            if (correctInputs())
                CreateProduct();
        }
        private void PreviewTextInput_TbUnitPrice(object sender, TextCompositionEventArgs e)
        {
            InputUtilities.ValidateMonetaryInput(sender, e);
        }

        private void LostFocus_TbUnitPrice(object sender, RoutedEventArgs e)
        {
            InputUtilities.FormatMonetaryValue(sender, e);
        }
    }
}
