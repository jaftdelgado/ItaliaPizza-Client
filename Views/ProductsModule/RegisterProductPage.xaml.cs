using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.Views.RecipesModule;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Views.ProductsModule
{
    public partial class RegisterProductPage : Page
    {
        private Product _editingProduct;

        private Recipe _recipe;

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
            _recipe = editingProduct.Recipe;

            ConfigureInterfaceForMode();
            SetCategoriesComboBox();
            SetInputFields();
            LoadProductData(editingProduct);
            UpdateButtonState();

            BtnAddRecipe.IsEnabled = true;
            UpdateRecipeButtonsVisibility();
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditProduct_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditProduct_Desc");
                BtnEditProduct.Visibility = Visibility.Visible;
                BtnRegisterProduct.Visibility = Visibility.Collapsed;
                if (_editingProduct.ProductPic != null) BtnDeleteImage.IsEnabled = true;
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
            int productId = -1;
            string rawText = TbUnitPrice.Text.Replace(",", "").Replace("$", "").Trim();
            decimal price = decimal.TryParse(rawText, out decimal parsedPrice) ? parsedPrice : 0;

            var productDTO = new ProductDTO
            {
                Name = TbProductName.Text.Trim(),
                Category = (int)CbCategories.SelectedValue,
                Price = price,
                IsPrepared = !(NotIsPreparedCheckBox.IsChecked ?? false),
                ProductPic = GetProductPicData(),
                Description = TbDescription.Text.Trim(),
                RecipeID = null
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                productId = await client.AddProductAsync(productDTO);

                if (productDTO.IsPrepared == true && _recipe != null && productId > 0)
                {
                    _recipe.ProductID = productId;

                    var recipeDTO = new RecipeDTO
                    {
                        ProductID = _recipe.ProductID,
                        PreparationTime = _recipe.PreparationTime,
                        Steps = _recipe.Steps?.Select(s => new RecipeStepDTO
                        {
                            StepNumber = s.StepNumber,
                            Instruction = s.Instruction
                        }).ToArray(),
                        Supplies = _recipe.Supplies?.Select(s => new RecipeSupplyDTO
                        {
                            SupplyID = s.SupplyID,
                            UseQuantity = s.UseQuantity
                        }).ToArray()
                    };

                    int recipeId = await client.AddRecipeAsync(recipeDTO);

                    if (recipeId > 0)
                    {
                        productDTO.ProductID = productId; 
                        productDTO.RecipeID = recipeId;
                        await client.UpdateProductAsync(productDTO);
                    }
                }
            });

            if (productId > 0)
            {
                MessageDialog.Show("RegProduct_DialogTSuccess", "RegProduct_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private async Task UpdateProduct()
        {
            string rawText = TbUnitPrice.Text.Replace(",", "").Replace("$", "").Trim();
            decimal price = decimal.TryParse(rawText, out decimal parsedPrice) ? parsedPrice : 0;

            var productDTO = new ProductDTO
            {
                ProductID = _editingProduct.ProductID,
                Name = TbProductName.Text.Trim(),
                Category = (int)CbCategories.SelectedValue,
                Price = price,
                IsPrepared = !(NotIsPreparedCheckBox.IsChecked ?? false),
                ProductPic = GetProductPicData(),
                Description = TbDescription.Text.Trim(),
                RecipeID = _editingProduct.RecipeID
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                if (_editingProduct.Recipe != null && _recipe == null)
                {
                    // Eliminar receta anterior
                    await client.DeleteRecipeAsync(_editingProduct.RecipeID.Value);
                    productDTO.RecipeID = null;
                }
                else if (_editingProduct.Recipe == null && _recipe != null)
                {
                    // Crear nueva receta
                    _recipe.ProductID = _editingProduct.ProductID;
                    var recipeDTO = new RecipeDTO
                    {
                        ProductID = _recipe.ProductID,
                        PreparationTime = _recipe.PreparationTime,
                        Steps = _recipe.Steps?.Select(s => new RecipeStepDTO
                        {
                            StepNumber = s.StepNumber,
                            Instruction = s.Instruction
                        }).ToArray(),
                        Supplies = _recipe.Supplies?.Select(s => new RecipeSupplyDTO
                        {
                            SupplyID = s.SupplyID,
                            UseQuantity = s.UseQuantity
                        }).ToArray()
                    };

                    int newRecipeID = await client.AddRecipeAsync(recipeDTO);
                    if (newRecipeID > 0)
                    {
                        productDTO.RecipeID = newRecipeID;
                    }
                }
                else if (_editingProduct.Recipe != null && _recipe != null)
                {
                    // Actualizar receta existente
                    _recipe.ProductID = _editingProduct.ProductID;
                    _recipe.Id = _editingProduct.RecipeID.Value;

                    var recipeDTO = new RecipeDTO
                    {
                        RecipeID = _recipe.Id,
                        ProductID = _recipe.ProductID,
                        PreparationTime = _recipe.PreparationTime,
                        Steps = _recipe.Steps?.Select(s => new RecipeStepDTO
                        {
                            StepNumber = s.StepNumber,
                            Instruction = s.Instruction
                        }).ToArray(),
                        Supplies = _recipe.Supplies?.Select(s => new RecipeSupplyDTO
                        {
                            SupplyID = s.SupplyID,
                            UseQuantity = s.UseQuantity
                        }).ToArray()
                    };

                    var updated = await client.UpdateRecipeAsync(recipeDTO);

                    if (updated)
                    {
                        // Importante: actualizar el RecipeID en productDTO
                        productDTO.RecipeID = _editingProduct.RecipeID;
                    }
                    else
                    {
                        MessageDialog.Show("Error al actualizar la receta.", "Error", AlertType.ERROR);
                        return;
                    }
                }

                // Actualizar el producto con el RecipeID correcto
                await client.UpdateProductAsync(productDTO);
            });

            MessageDialog.Show("EditProduct_DialogTSuccess", "EditProduct_DialogDSuccess", AlertType.SUCCESS,
                () => NavigationManager.Instance.GoBack());
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

        private void UpdateRecipeButtonsVisibility()
        {
            if (_recipe != null)
            {
                BtnAddRecipe.Visibility = Visibility.Collapsed;
                ManageRecipesButtons.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAddRecipe.Visibility = Visibility.Visible;
                ManageRecipesButtons.Visibility = Visibility.Collapsed;
            }
        }


        private async void Click_BtnRegisterProduct(object sender, RoutedEventArgs e)
        {
            await RegisterProduct();
        }

        private async void Click_BtnEditProduct(object sender, RoutedEventArgs e)
        {
            await UpdateProduct();
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

            if (_isEditMode)
            {
                productToSend = _editingProduct;
            }
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

            var recipePage = new RegisterRecipePage(productToSend);
            recipePage.RecipeAssociated += OnRecipeAssociated;

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPage("RegRecipe_Header", recipePage);
            }
        }

        private void OnRecipeAssociated(Recipe recipe)
        {
            _recipe = recipe;

            if (_recipe != null)
            {
                BtnAddRecipe.Visibility = Visibility.Collapsed;
                ManageRecipesButtons.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAddRecipe.Visibility = Visibility.Visible;
                ManageRecipesButtons.Visibility = Visibility.Collapsed;
            }
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

        private void Click_BtnDeleteRecipe(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnEditRecipe(object sender, RoutedEventArgs e)
        {
            if (_recipe == null) return;

            var recipePage = new RegisterRecipePage(_editingProduct ?? new Product
            {
                Name = TbProductName.Text.Trim(),
                Category = CbCategories.SelectedIndex + 1,
                ProductPic = GetProductPicData(),
                Description = TbDescription.Text.Trim()
            }, _recipe);

            recipePage.RecipeAssociated += OnRecipeAssociated;

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToPage("RegRecipe_Header", recipePage);
            }
        }
    }
}