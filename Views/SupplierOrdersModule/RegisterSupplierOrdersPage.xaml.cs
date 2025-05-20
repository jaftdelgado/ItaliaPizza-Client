using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.SupplierOrdersModule
{
    public partial class RegisterSupplierOrdersPage : Page
    {
        private SupplierOrder _editingOrder;
        private bool _isEditMode;
        public ObservableCollection<SupplyCard> SupplyCards { get; set; } = new ObservableCollection<SupplyCard>();
        private ObservableCollection<Supplier> _suppliers = new ObservableCollection<Supplier>();
        private List<OrderedSupply> _orderedSupplies = new List<OrderedSupply>();
        private SupplyCategory _previousCategory;
        private Supplier _previousSupplier;

        public RegisterSupplierOrdersPage()
        {
            InitializeComponent();
            SetCategoriesComboBox();
            UpdateButtonState(_isEditMode ? BtnEditOrder : BtnConfirmOrder);
            ConfigureInterfaceForMode();
            UpdateTotal();
            OrderDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        public RegisterSupplierOrdersPage(SupplierOrder editingOrder)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingOrder = editingOrder;

            Loaded += async (s, e) =>
            {
                await ServiceClientManager.ExecuteServerAction(async () =>
                {
                    await LoadOrderDataInSingleOperation(editingOrder);
                });
            };

            ConfigureInterfaceForMode();
            SetCategoriesComboBox();
            UpdateButtonState(_isEditMode ? BtnEditOrder : BtnConfirmOrder);
        }

        private async Task LoadOrderDataInSingleOperation(SupplierOrder editingOrder)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CbCategories.SelectedIndex = editingOrder.CategorySupplyID - 1;
                OrderDate.Text = editingOrder.OrderedDateFormatted;
            });

            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;

            var suppliersTask = Task.Run(() => client.GetSuppliersByCategory(editingOrder.CategorySupplyID));
            var suppliesTask = Task.Run(() => client.GetSuppliesBySupplier(editingOrder.SupplierID));

            await Task.WhenAll(suppliersTask, suppliesTask);

            var suppliers = suppliersTask.Result.Select(s => new Supplier
            {
                Id = s.Id,
                SupplierName = s.SupplierName,
                ContactName = s.ContactName,
                PhoneNumber = s.PhoneNumber,
                EmailAddress = s.EmailAddress,
                Description = s.Description,
                CategorySupply = s.CategorySupply,
                IsActive = s.IsActive
            }).Where(s => s.IsActive).OrderBy(s => s.SupplierName).ToList();

            var supplies = suppliesTask.Result.Select(s => new Supply
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                MeasureUnit = s.MeasureUnit,
                Stock = s.Stock,
                Brand = s.Brand,
                SupplyPic = s.SupplyPic,
                SupplierID = s.SupplierID
            }).ToList();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var selectedSupplier = new Supplier
                {
                    Id = editingOrder.SupplierID,
                    SupplierName = editingOrder.SupplierName
                };

                _suppliers.Clear();

                foreach (var supplier in suppliers)
                {
                    _suppliers.Add(supplier);
                }

                CbSuppliers.ItemsSource = _suppliers;

                CbSuppliers.SelectedItem = _suppliers.FirstOrDefault(s => s.Id == selectedSupplier.Id);
                CbSuppliers.IsEnabled = false;

                CbSuppliers.DisplayMemberPath = "SupplierName";

                TbSupplier.Text = selectedSupplier.SupplierName;

                SupplyCards.Clear();
                foreach (var supply in supplies)
                {
                    var card = CreateSupplyCard(supply);
                    SupplyCards.Add(card);
                }
                ButtonsPanel.ItemsSource = SupplyCards;

                _orderedSupplies.Clear();
                OrderDetailsPanel.Children.Clear();
                foreach (var item in editingOrder.Items)
                {
                    var supply = supplies.FirstOrDefault(s => s.Id == item.Supply.Id);
                    if (supply != null)
                    {
                        var orderedSupply = new OrderedSupply
                        {
                            Supply = supply,
                            Quantity = item.Quantity
                        };
                        _orderedSupplies.Add(orderedSupply);

                        var detail = AddSupplyOrderDetail(orderedSupply);
                        OrderDetailsPanel.Children.Add(detail);
                    }
                }

                UpdateButtonState(BtnEditOrder);
                UpdateTotal();
            });
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditOrderSupplier_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditOrderSupplier_Desc");
                BtnEditOrder.Visibility = Visibility.Visible;
                BtnConfirmOrder.Visibility = Visibility.Collapsed;
                CbCategories.IsEnabled = false;
                CbSuppliers.IsEnabled = false;
            }
            else
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegOrderSupplier_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegOrderSupplier_Desc");
                BtnEditOrder.Visibility = Visibility.Collapsed;
            }
        }

        private void SetCategoriesComboBox()
        {
            CbCategories.ItemsSource = SupplyCategory.GetDefaultSupplyCategories();
        }

        private void ClearOrderDetails()
        {
            SupplyCards.Clear();
            _orderedSupplies.Clear();
            OrderDetailsPanel.Children.Clear();
            UpdateButtonState(_isEditMode ? BtnEditOrder : BtnConfirmOrder);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = _orderedSupplies.Sum(o => o.Quantity * o.Supply.Price);
            TbTotal.Text = total.ToString("C");
        }

        private async Task ConfirmAndClearOrderDetails(Func<Task> continueAction, Action onCancel = null)
        {
            if (_orderedSupplies.Count == 0)
            {
                await continueAction();
                return;
            }

            MessageDialog.ShowConfirm(
                "RegOrderSupplier_DialogTDialogTSelection", "RegOrderSupplier_DialogDDialogTSelection",
                async () =>
                {
                    ClearOrderDetails();
                    await continueAction();
                });

            MessageDialog.OnCancel = onCancel;
        }

        private void UpdateButtonState(Button button)
        {
            button.IsEnabled = _orderedSupplies.Count > 0;
        }

        private async Task LoadSuppliers()
        {
            var selectedCategory = CbCategories.SelectedItem as SupplyCategory;
            if (selectedCategory == null) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var suppliers = client.GetSuppliersByCategory(selectedCategory.Id)
                    .Select(s => new Supplier
                    {
                        Id = s.Id,
                        SupplierName = s.SupplierName,
                        ContactName = s.ContactName,
                        PhoneNumber = s.PhoneNumber,
                        EmailAddress = s.EmailAddress,
                        Description = s.Description,
                        CategorySupply = s.CategorySupply,
                        IsActive = s.IsActive
                    })
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SupplierName)
                    .ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _suppliers.Clear();
                    foreach (var supplier in suppliers)
                        _suppliers.Add(supplier);

                    CbSuppliers.ItemsSource = _suppliers.ToList();
                });
            });
        }

        private async Task LoadSuppliesForSelectedSupplier(IEnumerable<OrderedSupply> orderedSupplies = null)
        {
            var selectedSupplier = CbSuppliers.SelectedItem as Supplier;
            if (selectedSupplier == null) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var supplies = client.GetSuppliesBySupplier(selectedSupplier.Id)
                    .Select(s => new Supply
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Price = s.Price,
                        MeasureUnit = s.MeasureUnit,
                        Stock = s.Stock,
                        Brand = s.Brand,
                        SupplyPic = s.SupplyPic,
                        SupplierID = s.SupplierID
                    }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SupplyCards.Clear();
                    foreach (var supply in supplies)
                    {
                        SupplyCards.Add(CreateSupplyCard(supply));
                    }

                    ButtonsPanel.ItemsSource = SupplyCards;
                    TbSupplier.Text = _isEditMode ? TbSupplier.Text : selectedSupplier.SupplierName;

                    if (_isEditMode && orderedSupplies != null && orderedSupplies.Any())
                    {
                        _orderedSupplies.Clear();
                        OrderDetailsPanel.Children.Clear();

                        foreach (var item in orderedSupplies)
                        {
                            var supply = supplies.FirstOrDefault(s => s.Id == item.Supply.Id);
                            if (supply != null)
                            {
                                var orderedSupply = new OrderedSupply
                                {
                                    Supply = supply,
                                    Quantity = item.Quantity
                                };
                                _orderedSupplies.Add(orderedSupply);
                                OrderDetailsPanel.Children.Add(AddSupplyOrderDetail(orderedSupply));
                            }
                        }
                    }
                });
            });
        }

        private async Task RegisterSupplierOrder()
        {
            var selectedSupplier = CbSuppliers.SelectedItem as Supplier;

            if (selectedSupplier is null || !_orderedSupplies.Any())
            {
                MessageDialog.Show("RegOrderSupplier_DialogTInvalid", "RegOrderSupplier_DialogDInvalid", AlertType.WARNING);
                return;
            }

            var orderDto = new SupplierOrderDTO
            {
                SupplierID = selectedSupplier.Id,
                OrderedDate = DateTime.Now,
                Total = _orderedSupplies.Sum(o => o.Quantity * o.Supply.Price),
                Items = _orderedSupplies.Select(o => new SupplierOrderDTO.OrderItemDTO
                {
                    SupplyID = o.Supply.Id,
                    Quantity = o.Quantity,
                    Subtotal = o.Quantity * o.Supply.Price
                }).ToArray()
            };

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var result = await client.AddSupplierOrderAsync(orderDto);
                success = result > 0;
            });

            if (success)
            {
                MessageDialog.Show("RegOrderSupplier_DialogTSuccess", "RegOrderSupplier_DialogDSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private async Task UpdateSupplierOrder()
        {
            var orderDto = new SupplierOrderDTO
            {
                SupplierOrderID = _editingOrder.SupplierOrderID,
                SupplierID = _editingOrder.SupplierID,
                Total = _orderedSupplies.Sum(o => o.Quantity * o.Supply.Price),
                Items = _orderedSupplies.Select(o => new SupplierOrderDTO.OrderItemDTO
                {
                    SupplyID = o.Supply.Id,
                    Quantity = o.Quantity,
                    Subtotal = o.Quantity * o.Supply.Price
                }).ToArray()
            };

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                success = await client.UpdateSupplierOrderAsync(orderDto);
            });

            if (success)
            {
                MessageDialog.Show("RegOrderSupplier_DialogTEditSuccess", 
                    "RegOrderSupplier_DialogDEditSuccess", 
                    AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private SupplyCard CreateSupplyCard(Supply supply)
        {
            var card = new SupplyCard
            {
                SupplyName = supply.Name,
                StockText = $"Stock: {supply.Stock}",
                PriceText = $"${supply.Price:N2}",
                Margin = new Thickness(0, 0, 10, 10)
            };

            ImageUtilities.SetImageSource(card.SupplyPic, supply.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

            card.CardClicked += (_, __) => OnSupplyCardClicked(supply);

            return card;
        }

        private void RemoveOrderedSupply(SupplyOrderDetail detail, OrderedSupply orderedSupply)
        {
            Animations.BeginAnimation(detail, "HideBorderAnimation", () =>
            {
                OrderDetailsPanel.Children.Remove(detail);
                _orderedSupplies.Remove(orderedSupply);
                UpdateButtonState(_isEditMode ? BtnEditOrder : BtnConfirmOrder);
                UpdateTotal();
            });
        }

        private void OnSupplyCardClicked(Supply supply)
        {
            var existing = _orderedSupplies.FirstOrDefault(o => o.Supply.Id == supply.Id);

            if (existing != null)
            {
                existing.Quantity += 1;

                var detail = OrderDetailsPanel.Children
                    .OfType<SupplyOrderDetail>()
                    .FirstOrDefault(d => d.SupplyName == existing.Supply.Name);

                if (detail != null)
                {
                    detail.Quantity += 1;
                    detail.RefreshBinding();
                }
            }
            else
            {
                var ordered = new OrderedSupply
                {
                    Supply = supply,
                    Quantity = 1,
                };

                _orderedSupplies.Add(ordered);

                var detail = AddSupplyOrderDetail(ordered);
                OrderDetailsPanel.Children.Add(detail);

                Animations.BeginAnimation(detail, "PopupFadeInAnimation");
            }

            UpdateButtonState(_isEditMode ? BtnEditOrder : BtnConfirmOrder);
            UpdateTotal();
        }

        private SupplyOrderDetail AddSupplyOrderDetail(OrderedSupply orderedSupply)
        {
            var detail = new SupplyOrderDetail
            {
                SupplyName = orderedSupply.Supply.Name,
                Price = orderedSupply.Supply.Price,
                Quantity = orderedSupply.Quantity,
                MeasureUnitId = orderedSupply.Supply.MeasureUnit,
                Margin = new Thickness(0, 0, 0, 6),
                Subtotal = orderedSupply.Supply.Price * orderedSupply.Quantity,
                Unit = orderedSupply.Unit
            };

            detail.DeleteClicked += (_, __) => RemoveOrderedSupply(detail, orderedSupply);
            detail.QuantityChanged += (sender, newQuantity) =>
            {
                orderedSupply.Quantity = newQuantity;
                UpdateTotal();
            };

            ImageUtilities.SetImageSource(detail.SupplyPic, orderedSupply.Supply.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

            return detail;
        }

        private async void Click_BtnConfirmOrder(object sender, RoutedEventArgs e)
        {
            await RegisterSupplierOrder();
        }

        private async void Click_BtnEditOrder(object sender, RoutedEventArgs e)
        {
            await UpdateSupplierOrder();
        }

        private async void CbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newCategory = CbCategories.SelectedItem as SupplyCategory;
            if (newCategory == null || Equals(newCategory, _previousCategory)) return;

            CbCategories.SelectionChanged -= CbCategories_SelectionChanged;
            CbCategories.SelectedItem = _previousCategory;
            CbCategories.SelectionChanged += CbCategories_SelectionChanged;

            await ConfirmAndClearOrderDetails(async () =>
            {
                _previousCategory = newCategory;
                CbCategories.SelectionChanged -= CbCategories_SelectionChanged;
                CbCategories.SelectedItem = newCategory;
                CbCategories.SelectionChanged += CbCategories_SelectionChanged;

                TbSupplier.Text = "-";
                CbSuppliers.IsEnabled = newCategory != null;
                await LoadSuppliers();
            });
        }

        private async void CbSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newSupplier = CbSuppliers.SelectedItem as Supplier;
            if (newSupplier == null || Equals(newSupplier, _previousSupplier)) return;

            CbSuppliers.SelectionChanged -= CbSuppliers_SelectionChanged;
            CbSuppliers.SelectedItem = _previousSupplier;
            CbSuppliers.SelectionChanged += CbSuppliers_SelectionChanged;

            await ConfirmAndClearOrderDetails(async () =>
            {
                _previousSupplier = newSupplier;
                CbSuppliers.SelectionChanged -= CbSuppliers_SelectionChanged;
                CbSuppliers.SelectedItem = newSupplier;
                CbSuppliers.SelectionChanged += CbSuppliers_SelectionChanged;
                await LoadSuppliesForSelectedSupplier();
            });
        }
    }
}