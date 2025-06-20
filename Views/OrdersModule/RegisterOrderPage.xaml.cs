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

        private Customer _selectedCustomer;

        private int? _selectedDriverID;

        private Order _editingOrder;

        private bool _isEditMode;

        public RegisterOrderPage()
        {
            InitializeComponent();
            LoadInitialData();
            OrderDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            ConfigureInterfaceForMode(CurrentSession.LoggedInUser.RoleID);
            ResponsibleName.Text = CurrentSession.LoggedInUser.FullName;

            TbTableNumber.TextChanged += (_, __) => UpdateConfirmButtonState();
            UpdateConfirmButtonState();
            InputUtilities.ValidateInput(TbTableNumber, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_TABLE);
        }

        public RegisterOrderPage(Order editingOrder)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingOrder = editingOrder;

            Loaded += async (s, e) => await LoadOrderData(editingOrder);

            ConfigureInterfaceForMode(CurrentSession.LoggedInUser.RoleID);
            UpdateConfirmButtonState();
            InputUtilities.ValidateInput(TbTableNumber, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_TABLE);
        }

        private async void LoadInitialData()
        {
            await LoadProducts();

            if (CurrentSession.LoggedInUser.RoleID == 3)
            {
                await LoadDeliveryDrivers();
                await LoadCustomers();
            }
        }

        private async Task LoadProducts()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var products = client.GetProductsWithRecipe(false)
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
                    }).Where(p => p.IsActive).OrderBy(p => p.Category).ToList();

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

        private async Task LoadCustomers()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var customerDtos = await client.GetActiveCustomersAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _customers = customerDtos.Select(c => new Customer
                    {
                        CustomerID = c.CustomerID,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        EmailAddress = c.EmailAddress,
                        PhoneNumber = c.PhoneNumber,
                        RegDate = c.RegDate,
                        IsActive = c.IsActive,
                        AddressID = c.AddressID,
                        Address = c.Address == null ? null : new Address
                        {
                            Id = c.Address.Id,
                            AddressName = c.Address.AddressName,
                            ZipCode = c.Address.ZipCode,
                            City = c.Address.City
                        }
                    }).ToList();

                    CbCustomers.ItemsSource = _customers;
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
                MessageDialog.Show("RegLocalOrder_DialogTSuccess", "RegLocalOrder_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private async Task RegisterDeliveryOrder()
        {
            var orderDto = new OrderDTO
            {
                CustomerID = _selectedCustomer.CustomerID,
                PersonalID = CurrentSession.LoggedInUser.PersonalID,
                Total = _orderedProducts.Sum(o => o.Quantity * (o.Product.Price ?? 0)),
                Items = _orderedProducts.Select(o => new ProductOrderDTO
                {
                    ProductID = o.Product.ProductID,
                    Quantity = o.Quantity
                }).ToArray()
            };

            var deliveryDto = new DeliveryDTO
            {
                AddressID = _selectedCustomer.AddressID,
                DeliveryDriverID = _selectedDriverID.Value
            };

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var result = await client.AddDeliveryOrderAsync(orderDto, deliveryDto);
                success = result > 0;
            });

            if (success)
            {
                MessageDialog.Show("RegDeliverOrder_DialogTSuccess", "RegDeliverOrder_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private void ConfigureInterfaceForMode(int role)
        {
            if (role == 3) // Cajero
            {
                if (_isEditMode)
                {
                    PageHeader.SetResourceReference(TextBlock.TextProperty, "EditDeliveryOrder_Header");
                    PageDescription.SetResourceReference(TextBlock.TextProperty, "EditDeliveryOrder_Desc");
                }
                else
                {
                    PageHeader.SetResourceReference(TextBlock.TextProperty, "RegDeliveryOrder_Header");
                    PageDescription.SetResourceReference(TextBlock.TextProperty, "RegDeliveryOrder_Desc");
                }
                Responsible.SetResourceReference(TextBlock.TextProperty, "RegOrder_Cashier");
                DeliveryFields.Visibility = Visibility.Visible;
                TbTableNumber.Visibility = Visibility.Collapsed;
                TablePanel.Visibility = Visibility.Collapsed;
                AddressPanel.Visibility = Visibility.Visible;
                DeliveryPanel.Visibility = Visibility.Visible;

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
                TablePanel.Visibility = Visibility.Visible;
                AddressPanel.Visibility = Visibility.Collapsed;
                DeliveryPanel.Visibility = Visibility.Collapsed;

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
            bool hasCustomer = _selectedCustomer != null;
            bool hasDriver = _selectedDriverID != null;

            if (_isEditMode)
            {
                BtnEditOrder.IsEnabled = hasProducts;
            }
            else if (CurrentSession.LoggedInUser.RoleID == 4) // Mesero
            {
                BtnConfirmLocalOrder.IsEnabled = hasProducts && hasTableNumber;
            }
            else // Cajero
            {
                BtnConfirmDeliveryOrder.IsEnabled = hasProducts && hasCustomer && hasDriver;
            }
        }


        private async void Click_BtnConfirmLocalOrder(object sender, RoutedEventArgs e)
        {
            await RegisterLocalOrder();
        }

        private void TbTableNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            TableNumber.Text = string.IsNullOrWhiteSpace(TbTableNumber.Text)
                ? "-"
                : TbTableNumber.Text.Trim();
        }

        private void CbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = CbCustomers.SelectedItem as Customer;

            if (_selectedCustomer != null)
            {
                CustomerName.Text = _selectedCustomer.FullName;

                Address.Text = _selectedCustomer.Address != null
                    ? _selectedCustomer.FullAddress
                    : "-";
            }
            else
            {
                CustomerName.Text = "-";
                Address.Text = "-";
            }
            UpdateConfirmButtonState();
        }

        private void CbDelivery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbDelivery.SelectedItem is ComboBoxItem selectedItem)
            {
                _selectedDriverID = selectedItem.Tag as int?;
                DeliveryPersonal.Text = selectedItem.Content?.ToString() ?? "-";
            }
            else
            {
                _selectedDriverID = null;
                DeliveryPersonal.Text = "-";
            }

            UpdateConfirmButtonState();
        }

        private async void Click_BtnConfirmDeliveryOrder(object sender, RoutedEventArgs e)
        {
            await RegisterDeliveryOrder();
        }
        private async Task LoadOrderData(Order editingOrder)
        {
            await LoadProducts();

            if (CurrentSession.LoggedInUser.RoleID == 3)
            {
                await LoadDeliveryDrivers();
                await LoadCustomers();
            }

            OrderDate.Text = editingOrder.OrderDate?.ToString("dd/MM/yyyy") ?? "-";
            ResponsibleName.Text = CurrentSession.LoggedInUser.FullName;
            TbTableNumber.Text = editingOrder.TableNumber;
            TableNumber.Text = string.IsNullOrWhiteSpace(editingOrder.TableNumber)
                               ? "-"
                               : editingOrder.TableNumber;
            _orderedProducts.Clear();
            OrderDetailsPanel.Children.Clear();
            foreach (var item in editingOrder.Items)
            {
                var product = new Product
                {
                    ProductID = item.ProductID,
                    Name = item.Name,
                    Price = item.Price,
                    ProductPic = item.ProductPic
                };

                var orderedProduct = new OrderedProduct
                {
                    Product = product,
                    Quantity = item.Quantity
                };

                _orderedProducts.Add(orderedProduct);
                var detail = AddProductOrderDetail(orderedProduct);
                OrderDetailsPanel.Children.Add(detail);
            }
            UpdateTotal();
            if (editingOrder.CustomerID.HasValue)
            {
                _selectedCustomer = _customers
                    .FirstOrDefault(c => c.CustomerID == editingOrder.CustomerID.Value);

                if (_selectedCustomer != null)
                {
                    CbCustomers.SelectedItem = _selectedCustomer;
                    CbCustomers.IsEnabled = false;
                    CustomerName.Text = _selectedCustomer.FullName;
                }
            }
            if (editingOrder.DeliveryInfo != null)
            {
                int driverId = editingOrder.DeliveryInfo.DeliveryDriverID;

                foreach (ComboBoxItem item in CbDelivery.Items)
                {
                    if (item.Tag is int id && id == driverId)
                    {
                        CbDelivery.SelectedItem = item;
                        _selectedDriverID = driverId;
                        DeliveryPersonal.Text = editingOrder.DeliveryInfo.DeliveryDriverName ?? item.Content?.ToString();
                        break;
                    }
                }
            }

            BtnConfirmLocalOrder.Visibility = Visibility.Collapsed;
            BtnConfirmDeliveryOrder.Visibility = Visibility.Collapsed;
            BtnEditOrder.Visibility = Visibility.Visible;
        }
        private async void Click_BtnEditOrder(object sender, RoutedEventArgs e)
        {
            var order = new OrderDTO
            {
                OrderID = _editingOrder.OrderID,
                Total = _orderedProducts.Sum(o => o.Quantity * (o.Product.Price ?? 0)),
                Items = _orderedProducts.Select(o => new ProductOrderDTO
                {
                    ProductID = o.Product.ProductID,
                    Quantity = o.Quantity,
                    Price = o.Product.Price,
                    ProductPic = o.Product.ProductPic,
                    Name = o.Product.Name
                }).ToArray()
            };

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                success = await client.UpdateOrderAsync(order);
            });

            if (success)
            {
                MessageDialog.Show("Orden actualizada correctamente", "Los cambios fueron guardados con éxito.", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }
    }
}
