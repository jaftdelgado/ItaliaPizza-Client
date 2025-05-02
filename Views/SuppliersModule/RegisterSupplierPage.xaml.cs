using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterSupplierPage : Page
    {
        public RegisterSupplierPage()
        {
            InitializeComponent();
            SetCategoriesComboBox();
            SetInputFields();
            UpdateButtonState(BtnRegisterSupplier);
        }

        private void SetCategoriesComboBox()
        {
            CbCategories.ItemsSource = SupplyCategory.GetDefaultSupplyCategories();
        }

        private void SetInputFields()
        {
            var validations = new (TextBox, string, int)[]
            {
                (TbSupplierName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbContactName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbPhoneNumber, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_PHONENUMBER),
                (TbEmailAddress, Constants.EMAIL_ALLOWED_CHARS_PATTERN, Constants.MAX_LENGTH_EMAIL),
                (TbDescription, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_DESCRIPTION)
            };

            foreach (var (textBox, pattern, maxLength) in validations)
                InputUtilities.ValidateInput(textBox, pattern, maxLength);

            InputUtilities.ConvertToLowerCase(TbEmailAddress);
        }

        private void UpdateButtonState(Button button)
        {
            var requiredFields = new List<Control>
            {
                TbSupplierName,
                CbCategories,
                TbContactName,
                TbPhoneNumber,
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

            button.IsEnabled = allFieldsFilled;
        }

        private async Task LoadSuppliesAvailable(int categoryId)
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            var suppliesList = new List<Supply>();

            await ConnectionUtilities.ExecuteServerAction(async () =>
            {
                var supplies = await client.GetSuppliesAvailableByCategoryAsync(categoryId);

                suppliesList = supplies.Select(s => new Supply
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    MeasureUnit = s.MeasureUnit,
                    SupplyCategoryID = s.SupplyCategoryID,
                    Brand = s.Brand,
                    SupplierID = s.SupplierID,
                    Stock = s.Stock,
                    SupplyPic = s.SupplyPic,
                    Description = s.Description
                }).ToList();
            });

            SuppliesDataGrid.ItemsSource = suppliesList;
        }

        private async Task RegisterSupplier()
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            var supplierDto = new SupplierDTO
            {
                SupplierName = TbSupplierName.Text.Trim(),
                ContactName = TbContactName.Text.Trim(),
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                EmailAddress = string.IsNullOrWhiteSpace(TbEmailAddress.Text) ? null : TbEmailAddress.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(TbDescription.Text) ? null : TbDescription.Text.Trim(),
                CategorySupply = ((SupplyCategory)CbCategories.SelectedItem).Id
            };

            bool success = false;

            await ConnectionUtilities.ExecuteServerAction(async () =>
            {
                int result = await client.AddSupplierAsync(supplierDto);
                success = result > 0;
            });

            if (success)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("RegSupplier_DialogTSuccess", "RegSupplier_DialogDSuccess", AlertType.SUCCESS,
                        () =>
                        {
                            NavigationManager.Instance.GoBack();
                        });
                });
            }
        }


        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateButtonState(BtnRegisterSupplier);
        }

        private async void CbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCategory = CbCategories.SelectedItem as SupplyCategory;

            if (selectedCategory != null)
                await LoadSuppliesAvailable(selectedCategory.Id);
        }

        private async void Click_BtnRegisterSupplier(object sender, RoutedEventArgs e)
        {
            await RegisterSupplier();
        }
    }
}
