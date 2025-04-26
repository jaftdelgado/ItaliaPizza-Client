using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            SetMeasureComboBox();
            SetInputFields();
            UpdateFormButtonState(BtnRegisterSupply);
        }

        private void SetInputFields()
        {
            InputUtilities.ValidateInput(TbSupplyName, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbSupplyBrand, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_NAMES);

        }

        private void SelectProfileImage(Image targetImageControl, int targetWidth, int targetHeight)
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
                    if (!ImageUtilities.IsImageSizeValid(openFileDialog.FileName, Constants.MAX_IMAGE_SIZE))
                    {
                        MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                        return;
                    }

                    var resizedImage = ImageUtilities.LoadAndResizeImage(openFileDialog.FileName, targetWidth, targetHeight);
                    targetImageControl.Source = resizedImage;
                    BtnDeleteImage.IsEnabled = true;
                }
                catch (IOException)
                {
                    MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                }
            }
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            SelectProfileImage(SupplyImage, 180, 180);
        }

        private void SetCategoriesComboBox()
        {
            CbCategories.ItemsSource = SupplyCategory.GetDefaultSupplyCategories();
        }

        private void SetMeasureComboBox()
        {
            CbSupplyMeasure.ItemsSource = MeasureUnit.GetDefaultMeasureUnits();
        }

        private void UpdateFormButtonState(Button button)
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
            UpdateFormButtonState(BtnRegisterSupply);
        }

        private void CheckbBrand_Unchecked(object sender, RoutedEventArgs e)
        {
            TbSupplyBrand.IsEnabled = true;
            UpdateFormButtonState(BtnRegisterSupply);
        }

        private void PreviewTextInput_TbUnitPrice(object sender, TextCompositionEventArgs e)
        {
            InputUtilities.ValidateMonetaryInput(sender, e);
        }

        private void LostFocus_TbUnitPrice(object sender, RoutedEventArgs e)
        {
            InputUtilities.FormatMonetaryValue(sender, e);
        }

        private void Click_BtnRegisterSupply(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {

        }

        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateFormButtonState(BtnRegisterSupply);
        }
    }
}
