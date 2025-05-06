using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System;
using System.Collections.Generic;
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

namespace ItaliaPizzaClient.Views.CustomersModule
{
    public partial class CustomersPage : Page
    {
        private List<Customer> _allCustomers = new List<Customer>();
        private List<Customer> _filteredCustomers = new List<Customer>();

        public CustomersPage()
        {
            InitializeComponent();
            Loaded += CustomersPage_Loaded;
        }

        private void CustomersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCustomerData();
        }

        private async void LoadCustomerData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetCustomers();

                var list = dtoList.Select(c => new Customer
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    EmailAddress = c.EmailAddress,
                    PhoneNumber = c.PhoneNumber,
                    IsActive = c.IsActive,
                    AddressID = c.AddressID,
                    Address = c.Address == null ? null : new Address
                    {
                        Id = c.Address.Id,
                        AddressName = c.Address.AddressName,
                        ZipCode = c.Address.ZipCode,
                        City = c.Address.City
                    }
                })
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

                _allCustomers = list;
                _filteredCustomers = new List<Customer>(_allCustomers);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CustomerDataGrid.ItemsSource = _filteredCustomers;
                });
            });
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                _filteredCustomers = _allCustomers.Where(c =>
                $"{c.FirstName} {c.LastName}".ToLower().Contains(searchText) ||
                c.PhoneNumber?.ToLower().Contains(searchText) == true ||
                c.Address != null && (
                    c.Address.AddressName?.ToLower().Contains(searchText) == true ||
                    c.Address.City?.ToLower().Contains(searchText) == true ||
                    c.Address.ZipCode?.ToLower().Contains(searchText) == true
                    )
                ).ToList();
            }
            else
            {
                _filteredCustomers = new List<Customer>(_allCustomers);
            }

            if (!_allCustomers.Any())
            {
                if (string.IsNullOrWhiteSpace(searchText))
                    MessageBox.Show("no hay coincidencias");
            }

            CustomerDataGrid.ItemsSource = _filteredCustomers;
        }


        private void Click_BtnNewCustomer(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegCustomer_Header", new RegisterCustomerPage());
        }
    }
}
