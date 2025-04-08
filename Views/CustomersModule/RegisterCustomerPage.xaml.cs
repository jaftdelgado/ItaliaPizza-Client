using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterCustomerPage : Page
    {
        public RegisterCustomerPage()
        {
            InitializeComponent();
            
        }


        public void RegisterCustomer()
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            var customerDTO = new CustomerDTO
            {
                FirstName = TbCustomerName.Text.Trim(),
                LastName = TbCustomerLastName.Text.Trim(),
                PhoneNumber = TbCustomerPhone.Text
            };

            var addressDTO =new AddressDTO 
            {
                AddressName = TbAddressName.Text.Trim(),
                City = TbDistrict.Text.Trim(),
                ZipCode = TbZipcode.Text.Trim()
            };

            ConnectionUtilities.ExecuteDatabaseSafeAction(() =>
            {
                int result = client.RegisterCustomer(customerDTO,addressDTO);
                if (result > 0)
                {
                    MessageDialog.Show("Exito", "Registro exitoso", AlertType.SUCCESS);
                }
            });

            ConnectionUtilities.CloseClient();
        }

        private void BtnCancelRegister(object sender, RoutedEventArgs e)
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;
        }

        private void BtnSaveCustomer(object sender, RoutedEventArgs e)
        {
            RegisterCustomer();
        }
    }
}
