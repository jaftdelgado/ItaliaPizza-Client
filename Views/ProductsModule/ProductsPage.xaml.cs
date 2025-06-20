using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.ProductsModule
{
    public partial class ProductsPage : Page
    {
        private List<Product> _allProducts = new List<Product>();

        public ProductsPage()
        {
            InitializeComponent();
            BtnActive.Tag = "Selected";
            if (CurrentSession.LoggedInUser.RoleID != 1)
            {
                ManagePanel.Visibility = Visibility.Collapsed;
                BtnNewProduct.Visibility = Visibility.Collapsed;
            }
            
            Loaded += ProductsPage_Loaded;
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProductPanelVisibility(null);
            LoadProductsData();
        }

        private async void LoadProductsData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllProducts(false);

                var list = dtoList.Select(p => new Product
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    IsPrepared = p.IsPrepared,
                    ProductPic = p.ProductPic,
                    Description = p.Description,
                    ProductCode = p.ProductCode,
                    IsActive = p.IsActive,
                    SupplyID = p.SupplyID,
                    IsDeletable = p.IsDeletable,
                    RecipeID = p.RecipeID,

                    Recipe = p.Recipe == null ? null : new Recipe
                    {
                        Id = p.Recipe.RecipeID,
                        PreparationTime = p.Recipe.PreparationTime,
                        Steps = p.Recipe.Steps?.Select(rs => new RecipeStep
                        {
                            Id = rs.RecipeStepID,
                            RecipeID = rs.RecipeID,
                            StepNumber = rs.StepNumber,
                            Instruction = rs.Instruction
                        }).OrderBy(rs => rs.StepNumber).ToList() ?? new List<RecipeStep>(),

                        Supplies = p.Recipe.Supplies?.Select(rsp => new RecipeSupplyItem
                        {
                            Id = rsp.RecipeSupplyID,
                            RecipeID = rsp.RecipeID,
                            SupplyID = rsp.SupplyID,
                            UseQuantity = rsp.UseQuantity
                        }).ToList() ?? new List<RecipeSupplyItem>()
                    }
                }).OrderBy(p => p.Category).ToList();


                _allProducts = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter("BtnActive");
                });
            });
        }

        private void SearchProducts()
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            string selectedFilter = GetSelectedFilterButtonName();

            IEnumerable<Product> filteredList = _allProducts;

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
                    p.ProductCode?.ToLower().Contains(searchText) == true ||
                    p.CategoryName?.ToLower().Contains(searchText) == true
                );
            }

            //EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
                NoMatchesMessage.Visibility = Visibility.Visible;

            ProductsDataGrid.ItemsSource = filteredList;
            UpdateElementsCounter(filteredList.Count());
        }

        private void ApplyFilter(string buttonName)
        {
            IEnumerable<Product> filteredList = _allProducts;

            switch (buttonName)
            {
                case "BtnActive":
                    filteredList = _allProducts.Where(p => p.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = _allProducts.Where(p => !p.IsActive);
                    break;
            }

            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                NoMatchesMessage.Visibility = Visibility.Visible;
            }

            ProductsDataGrid.ItemsSource = filteredList;

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

            BtnDeleted.IsEnabled = _allProducts.Any(p => !p.IsActive);
            UpdateElementsCounter(filteredList.Count());
        }

        private void DisplayProductDetails(Product selected)
        {
            if (selected == null) return;
            UpdateProductPanelVisibility(selected);

            ImageUtilities.SetImageSource(ProductPic, selected.ProductPic, Constants.DEFAULT_PRODUCT_PIC_PATH);

            ProductName.Text = selected.Name;
            ProductCode.Text = selected.ProductCode;
            ProductCategory.Text = selected.CategoryName;
            ProductDescription.Text = selected.Description;

            BtnDeleted.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnEditProduct.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnReactivateProduct.Visibility = selected.IsActive ? Visibility.Collapsed : Visibility.Visible;
            BtnDeleteProduct.IsEnabled = selected.IsDeletable && selected.IsActive;
            BtnDeleteProduct.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnActive.Tag?.ToString() == "Selected") return "BtnActive";
            if (BtnDeleted.Tag?.ToString() == "Selected") return "BtnDeleted";
            return "BtnViewAll";
        }

        private void UpdateProductPanelVisibility(Product selected)
        {
            bool hasSelection = selected != null;

            ProductDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateElementsCounter(int count)
        {
            ElementsCounter.Content = count.ToString();
        }

        private void Clic_BtnNewProduct(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegProduct_Header", new RegisterProductPage());
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchProducts();
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selected)
                DisplayProductDetails(selected);
            else
                UpdateProductPanelVisibility(null);
        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }


        private void Click_BtnEditProduct(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            var productToEdit = ProductsDataGrid.SelectedItem as Product;

            if (mainWindow != null && productToEdit != null)
                mainWindow.NavigateToPage("EditProduct_Header", new RegisterProductPage(productToEdit));
        }

        private void Click_BtnDeleteProduct(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Products_DialogTDeleteProduct", "Products_DialogDDeleteProduct",
                async () =>
                {
                    if (ProductsDataGrid.SelectedItem is Product selected)
                        await DeleteProduct(selected);
                },
                "Glb_Delete"
            );
        }

        private async Task DeleteProduct(Product selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;
                bool canDelete = client.IsProductDeletable(selected.ProductID);
                if (!canDelete)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageDialog.Show("Products_DialogTUnableToDelete", "Products_DialogDUnableToDelete", AlertType.WARNING);
                    });
                    return;
                }
                bool success = client.DeleteProduct(selected.ProductID);
                if (!success) return;
                var item = _allProducts.FirstOrDefault(p => p.ProductID == selected.ProductID);
                if (item != null) item.IsActive = false;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string selectedFilter = GetSelectedFilterButtonName();
                    ApplyFilter(selectedFilter);
                    MessageDialog.Show(
                        "Products_DialogTDeletedProduct",
                        "Products_DialogDDeletedProduct",
                        AlertType.SUCCESS
                    );
                });
            });
        }

        private void Click_BtnReactivateProduct(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Products_DialogTReactivateProduct", "Products_DialogDReactivateProduct",
                async () =>
                {
                    if (ProductsDataGrid.SelectedItem is Product selected)
                        await ReactivateProduct(selected);
                }
            );
        }

        private async Task ReactivateProduct(Product selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.ReactivateProduct(selected.ProductID);
                if (!result) return;

                var item = _allProducts.FirstOrDefault(p => p.ProductID == selected.ProductID);
                if (item != null) item.IsActive = true;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (!_allProducts.Any(p => !p.IsActive))
                        ApplyFilter("BtnActive");
                    else
                        ApplyFilter(GetSelectedFilterButtonName());

                    DisplayProductDetails(selected);
                    MessageDialog.Show("Products_DialogTReactivatedProduct", "Products_DialogDReactivatedProduct", AlertType.SUCCESS);
                });
            });
        }
    }
}
