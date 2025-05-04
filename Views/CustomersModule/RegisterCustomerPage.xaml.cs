using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterCustomerPage.xaml
    /// </summary>
    public partial class RegisterCustomerPage : Page
    {
        private Customer _editingCustomer;
        private bool _isActive = false;
        private bool _isEditMode;
        public RegisterCustomerPage()
        {
            InitializeComponent();
            SetInputFields();
            UpdateRegisterButtonState();
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
    }
}
