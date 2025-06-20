using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.SuppliersModule
{
    public partial class SuppliersPage : Page
    {
        private List<Supplier> _allSuppliers = new List<Supplier>();

        public SuppliersPage()
        {
            InitializeComponent();
            BtnActive.Tag = "Selected";
            Loaded += SuppliersPage_Loaded;
        }

        private async void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSupplierPanelVisibility(null);
            await LoadSupplierData();
        }

        private async Task LoadSupplierData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllSuppliers();

                var list = dtoList.Select(s => new Supplier
                {
                    Id = s.Id,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    PhoneNumber = s.PhoneNumber,
                    EmailAddress = s.EmailAddress,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CategorySupply = s.CategorySupply
                })
                .OrderBy(s => s.SupplierName)
                .ToList();

                _allSuppliers = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter("BtnActive");
                });
            });
        }
        private async Task DeleteSupplier(Supplier selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool canDelete = await client.CanDeleteSupplierAsync(selected.Id);
                if (!canDelete)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageDialog.Show("Suppliers_DialogTUnableToDelete", "Suppliers_DialogDHasPendingOrders", AlertType.WARNING);
                    });
                    return;
                }

                bool success = await client.DeleteSupplierAsync(selected.Id);
                if (!success) return;

                var item = _allSuppliers.FirstOrDefault(p => p.Id == selected.Id);
                if (item != null) item.IsActive = false;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter(GetSelectedFilterButtonName());

                    MessageDialog.Show("Suppliers_DialogTDeletedSupplier", "Suppliers_DialogDDeletedSupplier", AlertType.SUCCESS);
                });
            });
        }
        private async Task ReactivateSupply(Supplier selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.ReactivateSupplier(selected.Id);
                if (!result) return;

                var item = _allSuppliers.FirstOrDefault(p => p.Id == selected.Id);
                if (item != null) item.IsActive = true;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (!_allSuppliers.Any(p => !p.IsActive))
                        ApplyFilter("BtnActive");
                    else
                        ApplyFilter(GetSelectedFilterButtonName());

                    DisplaySupplierDetails(selected);
                    MessageDialog.Show("Suppliers_DialogTReactivatedSupplier", "Suppliers_DialogDReactivatedSupplier", AlertType.SUCCESS);
                });
            });
        }

        private void ApplyFilter(string buttonName)
        {
            IEnumerable<Supplier> filteredList = _allSuppliers;

            switch (buttonName)
            {
                case "BtnActive":
                    filteredList = _allSuppliers.Where(p => p.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = _allSuppliers.Where(p => !p.IsActive);
                    break;
            }

            //EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                NoMatchesMessage.Visibility = Visibility.Visible;
            }

            SuppliersDataGrid.ItemsSource = filteredList;

            BtnActive.Tag = null;
            BtnDeleted.Tag = null;
            BtnViewAll.Tag = null;

            switch (buttonName)
            {
                case "BtnActive":
                    BtnActive.Tag = "Selected";
                    break;
                case "BtnDeleted":
                    BtnDeleted.Tag = "Selected";
                    break;
                case "BtnViewAll":
                    BtnViewAll.Tag = "Selected";
                    break;
            }

            BtnDeleted.IsEnabled = _allSuppliers.Any(p => !p.IsActive);
            UpdateElementsCounter(filteredList.Count());
        }

        private void DisplaySupplierDetails(Supplier selected)
        {
            if (selected == null)
                return;

            UpdateSupplierPanelVisibility(selected);

            SupplierName.Text = selected.SupplierName;
            ContactName.Text = selected.ContactName;
            SupplyCategory.Text = selected.CategoryName;
            SupplierDescription.Text = selected.Description;
            SupplierPhone.Text = selected.DisplayPhone;

            if (string.IsNullOrWhiteSpace(selected.EmailAddress)) EmailPanel.Visibility = Visibility.Collapsed;

            else
            {
                SupplierEmail.Text = selected.EmailAddress;
                EmailPanel.Visibility = Visibility.Visible;
            }

            BtnDeleteSupplier.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnEditSupplier.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnReactivateSupplier.Visibility = !selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnActive.Tag?.ToString() == "Selected") return "BtnActive";
            if (BtnDeleted.Tag?.ToString() == "Selected") return "BtnDeleted";
            return "BtnViewAll";
        }

        private void UpdateSupplierPanelVisibility(Supplier selected)
        {
            bool hasSelection = selected != null;

            SupplierDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateElementsCounter(int count)
        {
            ElementsCounter.Content = count.ToString();
        }

        private void Click_BtnNewSupplier(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
                mainWindow.NavigateToPage("RegSupplier_Header", new RegisterSupplierPage());
        }

        private void Click_BtnEditSupplier(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            var supplierToEdit = SuppliersDataGrid.SelectedItem as Supplier;

            if (mainWindow != null && supplierToEdit != null)
                mainWindow.NavigateToPage("EditSupplier_Header", new RegisterSupplierPage(supplierToEdit));
        }

        private void Click_BtnDeleteSupplier(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Suppliers_DialogTDeleteSupplier", "Suppliers_DialogDDeleteSupplier",
                async () =>
                {
                    if (SuppliersDataGrid.SelectedItem is Supplier selected)
                        await DeleteSupplier(selected);
                },
                "Glb_Delete"
            );
        }

        private void Click_BtnReactivateSupplier(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Suppliers_DialogTReactivateSupplier", "Suppliers_DialogDReactivateSupplier",
                async () =>
                {
                    if (SuppliersDataGrid.SelectedItem is Supplier selected)
                        await ReactivateSupply(selected);
                }
            );
        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }

        private void SuppliersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is Supplier selected)
                DisplaySupplierDetails(selected);
            else
                UpdateSupplierPanelVisibility(null);
        }
    }
}
