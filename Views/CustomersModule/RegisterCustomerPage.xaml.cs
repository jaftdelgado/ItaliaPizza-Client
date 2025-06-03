using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterCustomerPage : Page
    {
        private Customer _editingCustomer;
        private bool _isEditMode;
        public RegisterCustomerPage()
        {
            InitializeComponent();
            SetInputFields();
            UpdateRegisterButtonState();
        }

        public RegisterCustomerPage(Customer editingCustomer)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingCustomer = editingCustomer;

            ConfigureInterfaceForMode();
            SetInputFields();
            LoadCustomerData(editingCustomer);
            UpdateFormButtonState(_isEditMode ? BtnEditCustomer : BtnRegisterCostumer);
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditCustomer_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditCustomer_Desc");
                BtnEditCustomer.Visibility = Visibility.Visible;
                BtnRegisterCostumer.Visibility = Visibility.Collapsed;
            }
            else
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegCustomer_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegCustomer_Desc");
                BtnEditCustomer.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCustomerData(Customer customer)
        {
            TbCostumerName.Text = customer.FirstName;
            TbLastName.Text = customer.LastName;
            TbPhoneNumber.Text = customer.PhoneNumber;
            TbEmailAddress.Text = customer.EmailAddress;
            TbAddress.Text = customer.Address.AddressName;
            TbZipCode.Text = customer.Address.ZipCode;
            TbCity.Text = customer.Address.City;
        }

        private void SetInputFields()
        {
            InputUtilities.ValidateInput(TbCostumerName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbLastName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbPhoneNumber, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_PHONENUMBER);
            InputUtilities.ValidateInput(TbEmailAddress, Constants.EMAIL_ALLOWED_CHARS_PATTERN, Constants.MAX_LENGTH_EMAIL);
            InputUtilities.ValidateInput(TbAddress, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_ADDRESSNAME);
            InputUtilities.ValidateInput(TbZipCode, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_ZIPCODE);
            InputUtilities.ValidateInput(TbCity, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_CITY);

            InputUtilities.ConvertToLowerCase(TbEmailAddress);
        }

        private void UpdateFormButtonState(Button button)
        {
            var requiredFields = new List<TextBox>
            {
                TbCostumerName, TbLastName,
                TbPhoneNumber, TbEmailAddress,
                TbAddress, TbZipCode, TbCity
            };

            bool allFieldsFilled = true;

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    allFieldsFilled = false;
                    break;
                }
            }

            button.IsEnabled = allFieldsFilled;
        }

        private void UpdateRegisterButtonState()
        {
            var requiredFields = new List<object> {
                TbCostumerName,
                TbLastName,
                TbPhoneNumber,
                TbEmailAddress,
                TbAddress,
                TbZipCode,
                TbCity,
            };

            bool allFieldsFilled = true;
            foreach (TextBox field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    allFieldsFilled = false;
                    break;
                }
            }

            BtnRegisterCostumer.IsEnabled = allFieldsFilled;
        }

        public async Task RegisterCustomer()
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;

            var customerDTO = new CustomerDTO
            {
                FirstName = TbCostumerName.Text.Trim(),
                LastName = TbLastName.Text.Trim(),
                EmailAddress = TbEmailAddress.Text.Trim(),
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                Address = new AddressDTO
                {
                    AddressName = TbAddress.Text.Trim(),
                    ZipCode = TbZipCode.Text.Trim(),
                    City = TbCity.Text.Trim()
                }
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                if (!IsCustomerEmailAvailable(customerDTO.EmailAddress)) return;

                int result = client.AddCustomer(customerDTO);
                if (result > 0)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageDialog.Show("RegCostumer_DialogTSuccess", "RegCostumer_DialogDSuccess", AlertType.SUCCESS);
                        NavigationManager.Instance.GoBack();
                    });
                }
            });
        }

        private bool IsCustomerEmailAvailable(string email)
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return false;

            bool isCustomerEmailAvailable = client.IsCustomerEmailAvailable(email);
            if (!isCustomerEmailAvailable)
                MessageDialog.Show("RegCostumer_DialogTEmailDuplicate", "RegCostumer_DialogDEmailDuplicate", AlertType.WARNING);

            return isCustomerEmailAvailable;
        }

        #region EventHandlers
        private async void Click_BtnRegisterCostumer(object sender, RoutedEventArgs e)
        {
            if (InputUtilities.IsValidEmailFormat(TbEmailAddress.Text))
                await RegisterCustomer();
            else
                MessageDialog.Show("GlbDialogT_EmailFormat", "GlbDialogD_EmailFormat", AlertType.WARNING);
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }

        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateRegisterButtonState();
        }
        #endregion

        private async Task EditCustomer()
        {
            bool success = false;
            string validationErrorKey = null;
            string validationDescriptionKey = null;

            var updateDTO = new CustomerDTO
            {
                CustomerID = _editingCustomer.CustomerID,
                FirstName = _editingCustomer.FirstName,
                LastName = _editingCustomer.LastName,
                PhoneNumber = _editingCustomer.PhoneNumber,
                EmailAddress = _editingCustomer.EmailAddress,
                Address = new AddressDTO
                {
                    Id = _editingCustomer.AddressID,
                    AddressName = TbAddress.Text.Trim(),
                    ZipCode = TbZipCode.Text.Trim(),
                    City = TbCity.Text.Trim()
                }
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                if (!string.Equals(_editingCustomer.EmailAddress, updateDTO.EmailAddress,
                    StringComparison.OrdinalIgnoreCase) && !client.IsCustomerEmailAvailable(updateDTO.EmailAddress))
                {
                    validationErrorKey = "GlbDialogD_EmailDuplicate";
                    validationDescriptionKey = "GlbDialogD_EmailDuplicate";
                    return;
                }

                success = await client.UpdateCustomerAsync(updateDTO);
            });

            if (!string.IsNullOrEmpty(validationErrorKey))
            {
                MessageDialog.Show(validationErrorKey, validationDescriptionKey, AlertType.WARNING);
                return;
            }

            if (success)
            {
                MessageDialog.Show("RegCustomer_DialogTEditSuccess", "RegCustomer_DialogDEditSuccess",
                    AlertType.SUCCESS, () => NavigationManager.Instance.GoBack());
            }
        }

        private async void Click_BtnEditCustomer(object sender, RoutedEventArgs e)
        {
            await EditCustomer();
        }
    }
}
