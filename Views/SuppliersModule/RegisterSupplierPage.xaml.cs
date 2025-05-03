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
        private Supplier _editingSupplier;
        private bool _isEditMode;

        public RegisterSupplierPage()
        {
            InitializeComponent();
            SetCategoriesComboBox();
            SetInputFields();
            UpdateButtonState(BtnRegisterSupplier);
        }

        public RegisterSupplierPage(Supplier editingSupplier)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingSupplier = editingSupplier;

            ConfigureInterfaceForMode();
            SetCategoriesComboBox();
            SetInputFields();
            LoadSupplierData(editingSupplier);
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditSupplier_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditSupplier_Desc");
                BtnEditSupplier.Visibility = Visibility.Visible;
                BtnRegisterSupplier.Visibility = Visibility.Collapsed;
            }
            else
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegSupplier_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegSupplier_Desc");
                BtnEditSupplier.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSupplierData(Supplier editingSupplier)
        {
            TbSupplierName.Text = editingSupplier.SupplierName;
            TbEmailAddress.Text = editingSupplier.EmailAddress;
            TbDescription.Text = editingSupplier.Description;
            TbPhoneNumber.Text = editingSupplier.PhoneNumber;
            TbContactName.Text = editingSupplier.ContactName;

            CbCategories.SelectedIndex = editingSupplier.CategorySupply - 1;
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
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;

            var suppliesList = new List<Supply>();

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                int? supplierId = _isEditMode ? _editingSupplier?.Id : null;

                var supplies = await client.GetSuppliesAvailableByCategoryAsync(categoryId, supplierId);

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
                    Description = s.Description,
                    IsSelected = _isEditMode && s.SupplierID == _editingSupplier?.Id
                })
                .OrderByDescending(s => s.IsSelected)
                .ThenBy(s => s.Name)
                .ToList();
            });

            SuppliesDataGrid.ItemsSource = suppliesList;
        }

        private async Task RegisterSupplier()
        {
            bool success = false;

            var supplierDto = new SupplierDTO
            {
                SupplierName = TbSupplierName.Text.Trim(),
                ContactName = TbContactName.Text.Trim(),
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                EmailAddress = string.IsNullOrWhiteSpace(TbEmailAddress.Text) ? null : TbEmailAddress.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(TbDescription.Text) ? null : TbDescription.Text.Trim(),
                CategorySupply = ((SupplyCategory)CbCategories.SelectedItem).Id
            };

            int newSupplierId = -1;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                newSupplierId = await client.AddSupplierAsync(supplierDto);
                success = newSupplierId > 0;

                if (success)
                {
                    var selectedSupplies = SuppliesDataGrid.ItemsSource as List<Supply>;
                    var selectedSupplyIds = selectedSupplies?
                        .Where(s => s.IsSelected)
                        .Select(s => s.Id)
                        .ToList();

                    if (selectedSupplyIds?.Count > 0)
                    {
                        bool assignSuccess = await client.AssignSupplierToSupplyAsync(selectedSupplyIds.ToArray(), newSupplierId);
                        success = assignSuccess;
                    }
                }
            });

            if (success)
            {
                MessageDialog.Show("RegSupplier_DialogTSuccess", "RegSupplier_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private async Task EditSupplier()
        {
            bool success = false;

            var supplierDto = new SupplierDTO
            {
                Id = _editingSupplier.Id,
                SupplierName = TbSupplierName.Text.Trim(),
                ContactName = TbContactName.Text.Trim(),
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                EmailAddress = string.IsNullOrWhiteSpace(TbEmailAddress.Text) ? null : TbEmailAddress.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(TbDescription.Text) ? null : TbDescription.Text.Trim(),
                CategorySupply = ((SupplyCategory)CbCategories.SelectedItem).Id
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                success = await client.UpdateSupplierAsync(supplierDto);

                if (success)
                    success = await AssignSuppliesToSupplier(supplierDto.Id);
            });

            if (success)
            {
                MessageDialog.Show("RegSupplier_DialogTEditSuccess", "RegSupplier_DialogDEditSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private async Task<bool> AssignSuppliesToSupplier(int supplierId)
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return false;

            var selectedSupplies = SuppliesDataGrid.ItemsSource as List<Supply>;
            var selectedSupplyIds = selectedSupplies?
                .Where(s => s.IsSelected).Select(s => s.Id).ToList();

            if (selectedSupplyIds?.Count > 0)
                return await client.AssignSupplierToSupplyAsync(selectedSupplyIds.ToArray(), supplierId);

            return true;
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

        private async void Click_BtnEditSupplier(object sender, RoutedEventArgs e)
        {
            await EditSupplier();
        }

    }
}