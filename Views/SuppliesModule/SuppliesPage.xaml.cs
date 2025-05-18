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
    public partial class SuppliesPage : Page
    {
        private List<Supply> _allSupplies = new List<Supply>();

        public SuppliesPage()
        {
            InitializeComponent();
            BtnActive.Tag = "Selected";
            Loaded += SupplyPage_Loaded;
        }

        private void SupplyPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSupplyPanelVisibility(null);
            LoadSuppliesData();
        }

        private async void LoadSuppliesData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllSupplies(false);

                var list = dtoList.Select(s => new Supply
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    MeasureUnit = s.MeasureUnit,
                    Brand = s.Brand,
                    SupplyPic = s.SupplyPic,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    SupplyCategoryID = s.SupplyCategoryID,
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName,
                    CanBeDeleted = s.IsDeletable
                })
                .OrderBy(p => p.SupplyCategoryID)
                .ToList();

                _allSupplies = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter("BtnActive");
                });
            });
        }

        private async Task DeleteSupply(Supply selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool canDelete = client.IsSupplyDeletable(selected.Id);
                if (!canDelete)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageDialog.Show("Supplies_DialogTUnableToDelete", "Supplies_DialogDUnableToDelete", AlertType.WARNING
                        );
                    });
                    return;
                }

                bool success = client.DeleteSupply(selected.Id);
                if (!success) return;

                var item = _allSupplies.FirstOrDefault(p => p.Id == selected.Id);
                if (item != null) item.IsActive = false;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string selectedFilter = GetSelectedFilterButtonName();
                    ApplyFilter(selectedFilter);

                    MessageDialog.Show(
                        "Supplies_DialogTDeletedSupply",
                        "Supplies_DialogDDeletedSupply",
                        AlertType.SUCCESS
                    );
                });
            });
        }

        private async Task ReactivateSupply(Supply selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.ReactivateSupply(selected.Id);
                if (!result) return;

                var item = _allSupplies.FirstOrDefault(p => p.Id == selected.Id);
                if (item != null) item.IsActive = true;
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (!_allSupplies.Any(p => !p.IsActive))
                        ApplyFilter("BtnActive");
                    else
                        ApplyFilter(GetSelectedFilterButtonName());

                    DisplaySupplyDetails(selected);
                    MessageDialog.Show("Supplies_DialogTReactivatedSupply", "Supplies_DialogDReactivatedSupply", AlertType.SUCCESS);
                });
            });
        }

        private void SearchSupplies()
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            string selectedFilter = GetSelectedFilterButtonName();

            IEnumerable<Supply> filteredList = _allSupplies;

            switch (selectedFilter)
            {
                case "BtnActive":
                    filteredList = filteredList.Where(p => p.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = filteredList.Where(p => !p.IsActive);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredList = filteredList.Where(p =>
                    $"{p.Name}".ToLower().Contains(searchText) ||
                    p.Brand?.ToLower().Contains(searchText) == true ||
                    p.CategoryName?.ToLower().Contains(searchText) == true ||
                    p.SupplierName?.ToLower().Contains(searchText) == true
                );
            }

            //EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                NoMatchesMessage.Visibility = Visibility.Visible;
            }

            SupplyDataGrid.ItemsSource = filteredList;
            UpdateElementsCounter(filteredList.Count());
        }

        private void ApplyFilter(string buttonName)
        {
            IEnumerable<Supply> filteredList = _allSupplies;

            switch (buttonName)
            {
                case "BtnActive":
                    filteredList = _allSupplies.Where(p => p.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = _allSupplies.Where(p => !p.IsActive);
                    break;
            }

            //EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text));
                    //EmptyListMessage.Visibility = Visibility.Visible;
                else
                    NoMatchesMessage.Visibility = Visibility.Visible;
            }

            SupplyDataGrid.ItemsSource = filteredList;

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

            BtnDeleted.IsEnabled = _allSupplies.Any(p => !p.IsActive);
            UpdateElementsCounter(filteredList.Count());
        }

        private void DisplaySupplyDetails(Supply selected)
        {
            if (selected == null) return;

            UpdateSupplyPanelVisibility(selected);

            ImageUtilities.SetImageSource(SupplyPic, selected.SupplyPic, Constants.DEFAULT_PROFILE_PIC_PATH);

            SupplyBrand.Visibility = string.IsNullOrWhiteSpace(selected.Brand)
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (!string.IsNullOrWhiteSpace(selected.Brand))
                SupplyBrand.Text = selected.Brand.ToUpper() + "®";

            SupplyName.Text = selected.Name;
            SupplyCategory.Text = selected.CategoryName;
            SupplierName.Text = selected.Supplier;
            SupplyDescription.Text = selected.Description;

            BtnDeleteSupply.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnEditSupply.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnReactivateSupply.Visibility = !selected.IsActive ? Visibility.Visible : Visibility.Collapsed;

            BtnDeleteSupply.IsEnabled = selected.CanBeDeleted;
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnActive.Tag?.ToString() == "Selected") return "BtnActive";
            if (BtnDeleted.Tag?.ToString() == "Selected") return "BtnDeleted";
            return "BtnViewAll";
        }

        private void UpdateSupplyPanelVisibility(Supply selected)
        {
            bool hasSelection = selected != null;

            SupplyDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateElementsCounter(int count)
        {
            ElementsCounter.Content = count.ToString();
        }

        private void Click_BtnNewSupply(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegSupply_Header", new RegisterSupplyPage());
        }

        private void Click_BtnEditSupply(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            var supplyToEdit = SupplyDataGrid.SelectedItem as Supply;

            if (mainWindow != null && supplyToEdit != null)
                mainWindow.NavigateToPage("EditSupply_Header", new RegisterSupplyPage(supplyToEdit));

        }

        private void Click_BtnDeleteSupply(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Supplies_DialogTDeleteSupply", "Supplies_DialogDDeleteSupply",
                async () =>
                {
                    if (SupplyDataGrid.SelectedItem is Supply selected)
                        await DeleteSupply(selected);
                },
                "Glb_Delete"
            );
        }

        private void Click_BtnReactivateSupply(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Supplies_DialogTReactivateSupply", "Supplies_DialogDReactivateSupply",
                async () =>
                {
                    if (SupplyDataGrid.SelectedItem is Supply selected)
                        await ReactivateSupply(selected);
                }
            );
        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }

        private void SupplyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SupplyDataGrid.SelectedItem is Supply selected)
                DisplaySupplyDetails(selected);
            else
                UpdateSupplyPanelVisibility(null);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchSupplies();   
        }
    }
}