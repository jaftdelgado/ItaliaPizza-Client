using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.OrdersModule
{
    public partial class RegisterOrderPage : Page
    {
        private ObservableCollection<ProductCard> _productCards = new ObservableCollection<ProductCard>();

        private List<OrderedProduct> _orderedProducts = new List<OrderedProduct>();

        private List<Customer> _customers = new List<Customer>();

        public RegisterOrderPage()
        {
            InitializeComponent();
            LoadInitialData();
            OrderDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            ConfigureInterfaceForMode(CurrentSession.LoggedInUser.RoleID);
            ResponsibleName.Text = CurrentSession.LoggedInUser.FullName;

            TbTableNumber.TextChanged += (_, __) => UpdateConfirmButtonState();
            UpdateConfirmButtonState();
        }

        private async void LoadInitialData()
        {
            await LoadProducts();

            if (CurrentSession.LoggedInUser.RoleID == 3)
            {
                await LoadDeliveryDrivers();
            }
        }


        private async Task LoadProducts()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var products = client.GetProductsWithRecipe()
                    .Select(p => new Product
                    {
                        ProductID = p.ProductID,
                        Name = p.Name,
                        Price = p.Price,
                        ProductCode = p.ProductCode,
                        ProductPic = p.ProductPic,
                        IsActive = p.IsActive,
                        Category = p.Category,
                        Recipe = new Recipe
                        {
                            Id = p.Recipe.RecipeID,
                            PreparationTime = p.Recipe.PreparationTime
                        }
                    })
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Category)
                    .ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _productCards.Clear();
                    foreach (var product in products)
                    {
                        _productCards.Add(CreateProductCard(product));
                    }
                    ButtonsPanel.ItemsSource = _productCards;
                });
            });
        }

        private async Task LoadDeliveryDrivers()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var drivers = await client.GetDeliveryDriversAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CbDelivery.ItemsSource = drivers.Select(d => new ComboBoxItem
                    {
                        Content = $"{d.FirstName} {d.LastName}",
                        Tag = d.PersonalID
                    }).ToList();
                });
            });
        }

        private async Task RegisterLocalOrder()
        {
            var orderDto = new OrderDTO
            {
                TableNumber = TbTableNumber.Text?.Trim(),
                PersonalID = CurrentSession.LoggedInUser.PersonalID,
                Total = _orderedProducts.Sum(o => o.Quantity * (o.Product.Price ?? 0)),
                Items = _orderedProducts.Select(o => new ProductOrderDTO
                {
                    ProductID = o.Product.ProductID,
                    Quantity = o.Quantity
                }).ToArray()
            };

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var result = await client.AddLocalOrderAsync(orderDto);
                success = result > 0;
            });

            if (success)
            {
                MessageDialog.Show("RegOrder_DialogTSuccess", "RegOrder_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private void ConfigureInterfaceForMode(int role)
        {
            if (role == 3) // Cajero
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegDeliveryOrder_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegDeliveryOrder_Desc");
                Responsible.SetResourceReference(TextBlock.TextProperty, "RegOrder_Cashier");
                DeliveryFields.Visibility = Visibility.Visible;
                TbTableNumber.Visibility = Visibility.Collapsed;

                BtnConfirmLocalOrder.Visibility = Visibility.Collapsed;
                BtnConfirmDeliveryOrder.Visibility = Visibility.Visible;
            }
            else if (role == 4) // Mesero
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegOrder_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegOrder_Desc");
                Responsible.SetResourceReference(TextBlock.TextProperty, "RegOrder_Waiter");
                DeliveryFields.Visibility = Visibility.Collapsed;
                TbTableNumber.Visibility = Visibility.Visible;

                BtnConfirmLocalOrder.Visibility = Visibility.Visible;
                BtnConfirmDeliveryOrder.Visibility = Visibility.Collapsed;
            }
        }

        private ProductCard CreateProductCard(Product product)
        {
            var card = new ProductCard
            {
                ProductName = product.Name,
                ProductCode = product.ProductCode,
                PriceText = $"${product.Price:N2}",
                Margin = new Thickness(0, 0, 10, 10)
            };

            ImageUtilities.SetImageSource(card.ProductPic, product.ProductPic, Constants.DEFAULT_PRODUCT_PIC_PATH);

            card.CardClicked += (_, __) => OnProductCardClicked(product);

            return card;
        }

        private void OnProductCardClicked(Product product)
        {
            var existing = _orderedProducts.FirstOrDefault(o => o.Product.ProductID == product.ProductID);

            if (existing != null)
            {
                existing.Quantity += 1;

                var detail = OrderDetailsPanel.Children
                    .OfType<ProductOrderDetail>()
                    .FirstOrDefault(d => d.ProductName == existing.Product.Name);

                if (detail != null)
                {
                    detail.Quantity += 1;
                    detail.RefreshBinding();
                }
            }
            else
            {
                var ordered = new OrderedProduct
                {
                    Product = product,
                    Quantity = 1
                };

                _orderedProducts.Add(ordered);

                var detail = AddProductOrderDetail(ordered);
                OrderDetailsPanel.Children.Add(detail);

                Animations.BeginAnimation(detail, "PopupFadeInAnimation");
            }

            UpdateTotal();
            UpdateConfirmButtonState();
        }

        private ProductOrderDetail AddProductOrderDetail(OrderedProduct orderedProduct)
        {
            var detail = new ProductOrderDetail
            {
                ProductName = orderedProduct.Product.Name,
                Price = orderedProduct.Product.Price ?? 0,
                Quantity = orderedProduct.Quantity,
                Margin = new Thickness(0, 0, 0, 6),
                Subtotal = (orderedProduct.Product.Price ?? 0) * orderedProduct.Quantity
            };

            detail.DeleteClicked += (_, __) => RemoveOrderedProduct(detail, orderedProduct);
            detail.QuantityChanged += (sender, newQuantity) =>
            {
                orderedProduct.Quantity = newQuantity;
                UpdateTotal();
                UpdateConfirmButtonState();
            };

            ImageUtilities.SetImageSource(detail.ProductPic, orderedProduct.Product.ProductPic, Constants.DEFAULT_PRODUCT_PIC_PATH);

            return detail;
        }

        private void RemoveOrderedProduct(ProductOrderDetail detail, OrderedProduct orderedProduct)
        {
            Animations.BeginAnimation(detail, "HideBorderAnimation", () =>
            {
                OrderDetailsPanel.Children.Remove(detail);
                _orderedProducts.Remove(orderedProduct);
                UpdateTotal();
                UpdateConfirmButtonState();
            });
        }

        private void UpdateTotal()
        {
            decimal total = _orderedProducts.Sum(o => o.Quantity * (o.Product.Price ?? 0));
            TbTotal.Text = total.ToString("C");
        }

        private void UpdateConfirmButtonState()
        {
            bool hasProducts = _orderedProducts.Any();
            bool hasTableNumber = !string.IsNullOrWhiteSpace(TbTableNumber.Text);

            if (CurrentSession.LoggedInUser.RoleID == 4)
            {
                BtnConfirmLocalOrder.IsEnabled = hasProducts && hasTableNumber;
            }
            else 
            {
                BtnConfirmDeliveryOrder.IsEnabled = hasProducts;
            }
        }

        private async void Click_BtnConfirmOrder(object sender, RoutedEventArgs e)
        {
            await RegisterLocalOrder();
        }

        private void TbTableNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            TableNumber.Text = string.IsNullOrWhiteSpace(TbTableNumber.Text)
                ? "-"
                : TbTableNumber.Text.Trim();
        }

    }
}
