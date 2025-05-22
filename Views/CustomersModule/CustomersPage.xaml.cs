using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.CustomersModule
{
    public partial class CustomersPage : Page
    {
        private List<Customer> _allCustomers = new List<Customer>();
        private List<Customer> _filteredCustomers = new List<Customer>();

        public CustomersPage()
        {
            InitializeComponent();
            BtnActive.Tag = "Selected";
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
                    ApplyFilter("BtnActive");
                });
            });
        }

        private void ApplyFilter(string buttonName)
        {
            IEnumerable<Customer> filteredList = _allCustomers;

            switch (buttonName)
            {
                case "BtnActive":
                    filteredList = _allCustomers.Where(c => c.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = _allCustomers.Where(c => !c.IsActive);
                    break;
            }

            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                    NoMatchesMessage.Visibility = Visibility.Visible;
            }

            CustomerDataGrid.ItemsSource = filteredList;

            BtnActive.Tag = null;
            BtnDeleted.Tag = null;

            switch (buttonName)
            {
                case "BtnActive":
                    BtnActive.Tag = "Selected";
                    break;
                case "BtnDeleted":
                    BtnDeleted.Tag = "Selected";
                    break;
            }

            BtnDeleted.IsEnabled = _allCustomers.Any(c => !c.IsActive);
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnActive.Tag?.ToString() == "Selected") return "BtnActive";
            if (BtnDeleted.Tag?.ToString() == "Selected") return "BtnDeleted";
            return "BtnActive";
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

        private async Task DeleteCustomer(Customer selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool success = client.DeleteCustomer(selected.CustomerID);
                if (!success) return;

                var item = _allCustomers.FirstOrDefault(c => c.CustomerID == selected.CustomerID);
                if (item != null)
                    item.IsActive = false;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string selectedFilter = GetSelectedFilterButtonName();
                    ApplyFilter(selectedFilter);
                    MessageDialog.Show("Customers_DialogTDeletedCustomer", "Customers_DialogDDeletedCustomer", AlertType.SUCCESS);
                });
            });
        }

        private void UpdateCustomerPanelVisibility(Customer selected)
        {
            bool hasSelection = selected != null;

            CustomerDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DisplayCustomerDetails(Customer selected)
        {
            if (selected == null) return;

            UpdateCustomerPanelVisibility(selected);

            CustomerName.Text = selected.FullName;
            CustomerEmail.Text = selected.EmailAddress;
            CustomerAddress.Text = selected.FullAddress;

            BtnDeleteCustomer.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnEditCustomer.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnReactivateCustomer.Visibility = !selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerDataGrid.SelectedItem is Customer selected)
                DisplayCustomerDetails(selected);
            else
                UpdateCustomerPanelVisibility(null);
        }

        private void Click_BtnEditCustomer(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            var customerToEdit = CustomerDataGrid.SelectedItem as Customer;

            if (mainWindow != null && customerToEdit != null)
                mainWindow.NavigateToPage("EditCustomer_Header", new RegisterCustomerPage(customerToEdit));
        }

        private void Click_BtnDeleteCustomer(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Customers_DialogTDeleteCustomer", "Customers_DialogDDeleteCustomer",
                async () =>
                {
                    if (CustomerDataGrid.SelectedItem is Customer selected)
                        await DeleteCustomer(selected);
                },
                "Glb_Delete"
                );
        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }

        private async Task ReactivateCustomer(Customer selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.ReactivateCustomer(selected.CustomerID);
                if (!result) return;

                var item = _allCustomers.FirstOrDefault(c => c.CustomerID == selected.CustomerID);
                if (item != null) item.IsActive = true;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (!_allCustomers.Any(c => !c.IsActive))
                        ApplyFilter("BtnActive");
                    else
                        ApplyFilter(GetSelectedFilterButtonName());

                    DisplayCustomerDetails(selected);
                    MessageDialog.Show("Customers_DialogTReactivatedCustomer", "Customers_DialogDReactivatedCustomer",
                        AlertType.SUCCESS);
                });
            });
        }

        private void Click_BtnReactivateCustomer(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Customers_DialogTReactivateCustomer", "Customers_DialogDReactivateCustomer",
                async () =>
                {
                    if (CustomerDataGrid.SelectedItem is Customer selected)
                        await ReactivateCustomer(selected);
                }
            );
        }
    }
}
