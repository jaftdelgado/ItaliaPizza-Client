using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.Views.RecipesModule;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Views.ProductsModule
{
    public partial class RegisterProductPage : Page
    {
        private Product _editingProduct;
        private byte[] _selectedImageBytes;
        private bool _isEditMode;

        public RegisterProductPage()
        {
            InitializeComponent();
            ConfigureInterfaceForMode();
            SetInputFields();
            SetCategoriesComboBox();
            UpdateButtonState();
            _isEditMode = false;
            BtnAddRecipe.IsEnabled = false;
        }

        public RegisterProductPage(Product editingProduct)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingProduct = editingProduct;

            ConfigureInterfaceForMode();
            SetCategoriesComboBox();
            SetInputFields();
            LoadProductData(editingProduct);
            UpdateButtonState();

            if (editingProduct.ProductPic != null)
            {
                BtnDeleteImage.IsEnabled = true;
            }
            BtnAddRecipe.IsEnabled = true;
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditProduct_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditProduct_Desc");
                BtnEditProduct.Visibility = Visibility.Visible;
                BtnRegisterProduct.Visibility = Visibility.Collapsed;
            }
            else
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegProduct_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegProduct_Desc");
                BtnEditProduct.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadProductData(Product editingProduct)
        {
            TbProductName.Text = editingProduct.Name;
            TbUnitPrice.Text = editingProduct.FormattedPrice;
            TbDescription.Text = editingProduct.Description;

            CbCategories.SelectedIndex = (int)editingProduct.Category - 1;

            ImageUtilities.SetImageSource(ProductImage, editingProduct.ProductPic, Constants.DEFAULT_PROFILE_PIC_PATH);
        }

        private void SetInputFields()
        {
            var validations = new (TextBox, string, int)[]
            {
                (TbProductName, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbUnitPrice, Constants.MONETARY_VALUE_PATTERN, Constants.MAX_LENGTH_MONETARY_VALUE),
                (TbDescription, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_DESCRIPTION)
            };

            foreach (var (textBox, pattern, maxLength) in validations)
            {
                InputUtilities.ValidateInput(textBox, pattern, maxLength);
            }

            InputUtilities.ValidatePriceInput(TbUnitPrice);
        }

        private void SetCategoriesComboBox()
        {
            CbCategories.ItemsSource = ProductCategory.GetDefaultProductCategories();
        }

        private void SelectProductImage(Image targetImageControl)
        {
            var dialogTitle = Application.Current.Resources["RegProduct_DialogSelectProductPic"]?.ToString();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Constants.IMAGE_FILE_FILTER,
                Title = dialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var processedImageBytes = ImageUtilities.ProcessImageBeforeSaving(openFileDialog.FileName);

                    if (!ImageUtilities.IsImageSizeValid(processedImageBytes))
                    {
                        MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                        return;
                    }

                    _selectedImageBytes = processedImageBytes;
                    targetImageControl.Source = ImageUtilities.ConvertToImageSource(processedImageBytes);

                    BtnDeleteImage.IsEnabled = true;
                }
                catch (Exception)
                {
                    MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                }
            }
        }

        public byte[] GetProductPicData()
        {
            byte[] productPicData = null;

            if (ProductImage.Source is BitmapImage bitmapImage)
            {
                productPicData = ImageUtilities.ImageToByteArray(bitmapImage);

                var defaultImage = new BitmapImage(new Uri(Constants.DEFAULT_PRODUCT_PIC_PATH, UriKind.Absolute));
                var defaultImageBytes = ImageUtilities.ImageToByteArray(defaultImage);

                if (ImageUtilities.AreImagesEqual(productPicData, defaultImageBytes)) productPicData = null;
            }

            return productPicData;
        }

        private async Task RegisterProduct()
        {
            int success = -1;
            string rawText = TbUnitPrice.Text.Replace(",", "").Replace("$", "").Trim();
            decimal price = decimal.TryParse(rawText, out decimal parsedPrice) ? parsedPrice : 0;

            var productDTO = new ProductDTO
            {
                Name = TbProductName.Text.Trim(),
                Category = (int)CbCategories.SelectedValue,
                Price = price,
                IsPrepared = !(NotIsPreparedCheckBox.IsChecked ?? false),
                ProductPic = GetProductPicData(),
                Description = TbDescription.Text.Trim()
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                success = await client.AddProductAsync(productDTO);
            });

            if (success > 0)
            {
                MessageDialog.Show("RegProduct_DialogTSuccess", "RegProduct_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private void UpdateButtonState(Button button = null)
        {
            var requiredFields = new List<Control>
            {
                TbProductName,
                TbUnitPrice,
                TbDescription,
                CbCategories
            };

            bool allFieldsFilled = true;

            foreach (var field in requiredFields)
            {
                switch (field)
                {
                    case TextBox tb when string.IsNullOrWhiteSpace(tb.Text):
                        allFieldsFilled = false;
                        break;
                    case ComboBox cb when cb.SelectedItem == null:
                        allFieldsFilled = false;
                        break;
                }

                if (!allFieldsFilled) break;
            }

            if (_isEditMode) BtnEditProduct.IsEnabled = allFieldsFilled;

            else BtnRegisterProduct.IsEnabled = allFieldsFilled;

            BtnAddRecipe.IsEnabled = allFieldsFilled && !(NotIsPreparedCheckBox.IsChecked ?? false);
        }

        private async void Click_BtnRegisterProduct(object sender, RoutedEventArgs e)
        {
            await RegisterProduct();
        }

        private void Click_BtnEditProduct(object sender, RoutedEventArgs e)
        {
            // Implementación de edición de producto
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            SelectProductImage(ProductImage);
        }

        private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            ProductImage.Source = new BitmapImage(new Uri(Constants.DEFAULT_PRODUCT_PIC_PATH, UriKind.Absolute));
            _selectedImageBytes = null;
            BtnDeleteImage.IsEnabled = false;
        }

        private void RequiredFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState();
        }

        private void RequiredFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonState();
        }

        private void Click_BtnAddRecipe(object sender, RoutedEventArgs e)
        {
            Product productToSend;

            if (_isEditMode) productToSend = _editingProduct;

            else
            {
                productToSend = new Product
                {
                    Name = TbProductName.Text.Trim(),
                    Category = CbCategories.SelectedIndex + 1, 
                    ProductPic = GetProductPicData(),
                    Description = TbDescription.Text.Trim()
                };
            }

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
                mainWindow.NavigateToPage("RegRecipe_Header", new RegisterRecipePage(productToSend));
        }

        private void CheckbPrepared_Checked(object sender, RoutedEventArgs e)
        {
            BtnAddRecipe.Visibility = Visibility.Collapsed;
            BtnSelectSupply.Visibility = Visibility.Visible;
            TbRecipe.Visibility = Visibility.Collapsed;
            TbProductSupply.Visibility = Visibility.Visible;
            UpdateButtonState();
        }

        private void CheckbPrepared_Unchecked(object sender, RoutedEventArgs e)
        {
            BtnSelectSupply.Visibility = Visibility.Collapsed;
            BtnAddRecipe.Visibility = Visibility.Visible;
            TbProductSupply.Visibility = Visibility.Collapsed;
            TbRecipe.Visibility = Visibility.Visible;
            UpdateButtonState();
        }
    }
}