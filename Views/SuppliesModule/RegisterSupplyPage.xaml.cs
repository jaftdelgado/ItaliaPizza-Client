using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterSupplyPage : Page
    {
        private Supply _editingSupply;
        private byte[] _selectedImageBytes;
        private bool _isEditMode;

        public RegisterSupplyPage()
        {
            InitializeComponent();
            ConfigureInterfaceForMode();
            SetCategoriesComboBox();
            SetMeasureComboBox();
            SetInputFields();
            UpdateButtonState(BtnRegisterSupply);

            _isEditMode = false;
        }

        public RegisterSupplyPage(Supply editingSupply)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingSupply = editingSupply;

            ConfigureInterfaceForMode();
            SetCategoriesComboBox();
            SetMeasureComboBox();
            SetInputFields();
            LoadSupplyData(editingSupply);

            ImageUtilities.SetImageSource(SupplyImage, editingSupply.SupplyPic, Constants.DEFAULT_PROFILE_PIC_PATH);

            if (editingSupply.SupplyPic != null) BtnDeleteImage.IsEnabled = true;
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditSupply_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditSupply_Desc");
                BtnEditSupply.Visibility = Visibility.Visible;
                BtnRegisterSupply.Visibility = Visibility.Collapsed;
            }
            else
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegSupply_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegSupply_Desc");
                BtnEditSupply.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSupplyData(Supply editingSupply)
        {
            TbSupplyName.Text = editingSupply.Name;
            TbUnitPrice.Text = editingSupply.FormattedPrice;
            TbDescription.Text = editingSupply.Description;

            if (editingSupply.Brand != null) TbSupplyBrand.Text = editingSupply.Brand;
            else SupplyBrandCheckBox.IsChecked = true;

            CbCategories.SelectedIndex = editingSupply.SupplyCategoryID - 1;
            CbSupplyMeasure.SelectedIndex = editingSupply.MeasureUnit - 1;
        }

        private void SetInputFields()
        {
            var validations = new (TextBox, string, int)[]
            {
                (TbSupplyName, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbSupplyBrand, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbUnitPrice, Constants.MONETARY_VALUE_PATTERN, Constants.MAX_LENGTH_MONETARY_VALUE),
                (TbDescription, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_DESCRIPTION)
            };

            foreach (var (textBox, pattern, maxLength) in validations)
                InputUtilities.ValidateInput(textBox, pattern, maxLength);

            InputUtilities.ValidatePriceInput(TbUnitPrice);
        }

        private void SelectSupplyImage(Image targetImageControl)
        {
            var dialogTitle = Application.Current.Resources["RegEmployee_DialogSelectProfilePic"]?.ToString();

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

        public byte[] GetSupplyPicData()
        {
            byte[] supplyPicData = null;

            if (SupplyImage.Source is BitmapImage bitmapImage)
            {
                supplyPicData = ImageUtilities.ImageToByteArray(bitmapImage);

                var defaultImage = new BitmapImage(new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
                var defaultImageBytes = ImageUtilities.ImageToByteArray(defaultImage);

                if (ImageUtilities.AreImagesEqual(supplyPicData, defaultImageBytes))
                {
                    supplyPicData = null;
                }
            }

            return supplyPicData;
        }

        public async Task RegisterSupply()
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            string rawText = TbUnitPrice.Text.Replace(",", "").Replace("$", "").Trim();
            decimal price = decimal.TryParse(rawText, out decimal parsedPrice) ? parsedPrice : 0;

            var supplyDto = new SupplyDTO
            {
                Name = TbSupplyName.Text.Trim(),
                Price = price,
                MeasureUnit = (int) CbSupplyMeasure.SelectedValue,
                SupplyCategoryID = (int) CbCategories.SelectedValue,
                Brand = SupplyBrandCheckBox.IsChecked == false ? TbSupplyBrand.Text.Trim() : null,
                SupplyPic = GetSupplyPicData(),
                Description = string.IsNullOrWhiteSpace(TbDescription.Text) ? null : TbDescription.Text.Trim()
            };

            bool success = false;

            await ConnectionUtilities.ExecuteServerAction(async () =>
            {
                int result = await client.AddSupplyAsync(supplyDto);
                success = result > 0;
            });

            if (success)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("RegSupply_DialogTSuccess", "RegSupply_DialogDSuccess", AlertType.SUCCESS,
                        () =>
                        {
                            NavigationManager.Instance.GoBack();
                        });
                });
            }
        }

        private async Task EditSupply()
        {
            byte[] profilePicData = GetSupplyPicData();
            string rawText = TbUnitPrice.Text.Replace(",", "").Replace("$", "").Trim();
            decimal price = decimal.TryParse(rawText, out decimal parsedPrice) ? parsedPrice : 0;
            bool success = false;

            var updatedDto = new SupplyDTO
            {
                Id = _editingSupply.Id,
                Name = TbSupplyName.Text.Trim(),
                Price = price,
                MeasureUnit = (int)CbSupplyMeasure.SelectedValue,
                SupplyCategoryID = (int)CbCategories.SelectedValue,
                Brand = SupplyBrandCheckBox.IsChecked == false ? TbSupplyBrand.Text.Trim() : null,
                SupplyPic = GetSupplyPicData(),
                Description = string.IsNullOrWhiteSpace(TbDescription.Text) ? null : TbDescription.Text.Trim()
            }; 

            await ConnectionUtilities.ExecuteServerAction(async () =>
            {
                var client = ConnectionUtilities.IsServerConnected();
                if (client == null) return;

                success = await client.UpdateSupplyAsync(updatedDto);
            });

            if (success)
            {
                MessageDialog.Show("RegSupply_DialogTEditSuccess", "RegSupply_DialogDEditSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            SelectSupplyImage(SupplyImage);
        }

        private void SetCategoriesComboBox()
        {
            CbCategories.ItemsSource = SupplyCategory.GetDefaultSupplyCategories();
        }

        private void SetMeasureComboBox()
        {
            CbSupplyMeasure.ItemsSource = MeasureUnit.GetDefaultMeasureUnits();
        }

        private void UpdateButtonState(Button button)
        {
            var requiredFields = new List<Control>
            {
                TbSupplyName,
                TbUnitPrice,
                CbSupplyMeasure,
                CbCategories
            };

            if (SupplyBrandCheckBox.IsChecked == false)
                requiredFields.Add(TbSupplyBrand);

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

            button.IsEnabled = allFieldsFilled;
        }

        private void CheckbBrand_Checked(object sender, RoutedEventArgs e)
        {
            TbSupplyBrand.IsEnabled = false;
            UpdateButtonState(BtnRegisterSupply);
        }

        private void CheckbBrand_Unchecked(object sender, RoutedEventArgs e)
        {
            TbSupplyBrand.IsEnabled = true;
            UpdateButtonState(BtnRegisterSupply);
        }

        private async void Click_BtnRegisterSupply(object sender, RoutedEventArgs e)
        {
            await RegisterSupply();
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void RequiredFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState(BtnRegisterSupply);
        }

        private void RequiredFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonState(BtnRegisterSupply);
        }

        private async void Click_BtnEditSupply(object sender, RoutedEventArgs e)
        {
            await EditSupply();
        }
    }
}
