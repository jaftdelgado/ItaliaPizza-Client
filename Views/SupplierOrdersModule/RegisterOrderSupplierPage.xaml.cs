using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.UserControls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using ItaliaPizzaClient.Views.Dialogs;
using System.Threading.Tasks;
using System;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterOrderSupplierPage : Page
    {
        public ObservableCollection<SupplyCard> SupplyCards { get; set; } = new ObservableCollection<SupplyCard>();
        private ObservableCollection<Supplier> _suppliers = new ObservableCollection<Supplier>();
        private List<OrderedSupply> _orderedSupplies = new List<OrderedSupply>();
        private SupplyCategory _previousCategory;
        private Supplier _previousSupplier;

        public RegisterOrderSupplierPage()
        {
            InitializeComponent();
            SetCategoriesComboBox();
            UpdateButtonState(BtnConfirmOrder);
            UpdateTotal();
            TbCurrentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
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
            UpdateButtonState(BtnConfirmOrder);
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

                var dtoList = client.GetSuppliersByCategory(selectedCategory.Id);

                var suppliers = dtoList.Select(s => new Supplier
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
                .Where(s => s.IsActive).OrderBy(s => s.SupplierName).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _suppliers.Clear();
                    foreach (var supplier in suppliers)
                        _suppliers.Add(supplier);

                    CbSuppliers.ItemsSource = _suppliers.ToList();
                });
            });
        }

        private async Task LoadSuppliesForSelectedSupplier()
        {
            var selectedSupplier = CbSuppliers.SelectedItem as Supplier;
            if (selectedSupplier == null) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetSuppliesBySupplier(selectedSupplier.Id);

                var supplies = dtoList.Select(s => new Supply
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
                        var card = CreateSupplyCard(supply);
                        SupplyCards.Add(card);
                    }

                    ButtonsPanel.ItemsSource = SupplyCards;
                    TbSupplier.Text = selectedSupplier.SupplierName;
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

        private SupplyCard CreateSupplyCard(Supply supply)
        {
            var card = new SupplyCard
            {
                SupplyName = supply.SupplyName,
                StockText = $"Stock: {supply.Stock}",
                PriceText = supply.FormattedPricePerUnit,
                Margin = new Thickness(0, 0, 10, 10)
            };

            ImageUtilities.SetImageSource(card.SupplyPic, supply.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

            card.CardClicked += (_, __) => OnSupplyCardClicked(supply);

            return card;
        }

        private void RemoveOrderedSupply(SupplyOrderDetail detail, OrderedSupply orderedSupply)
        {
            OrderDetailsPanel.Children.Remove(detail);
            _orderedSupplies.Remove(orderedSupply);
            UpdateButtonState(BtnConfirmOrder);
            UpdateTotal();
        }

        private void OnSupplyCardClicked(Supply supply)
        {
            var existing = _orderedSupplies.FirstOrDefault(o => o.Supply.Id == supply.Id);

            if (existing != null)
            {
                existing.Quantity += 1;

                var detail = OrderDetailsPanel.Children
                    .OfType<SupplyOrderDetail>()
                    .FirstOrDefault(d => d.SupplyName == existing.Supply.SupplyName);

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
            }

            UpdateButtonState(BtnConfirmOrder);
            UpdateTotal();
        }

        private SupplyOrderDetail AddSupplyOrderDetail(OrderedSupply orderedSupply)
        {
            var detail = new SupplyOrderDetail
            {
                SupplyName = orderedSupply.Supply.SupplyName,
                Price = orderedSupply.Supply.Price,
                Quantity = orderedSupply.Quantity,
                MeasureUnitId = orderedSupply.Supply.MeasureUnit,
                Margin = new Thickness(0, 0, 0, 6)
            };

            detail.Subtotal = detail.Price * detail.Quantity;
            detail.Unit = orderedSupply.Unit;

            detail.DeleteClicked += (_, __) => RemoveOrderedSupply(detail, orderedSupply);

            ImageUtilities.SetImageSource(detail.SupplyPic, orderedSupply.Supply.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

            detail.QuantityChanged += (sender, newQuantity) =>
            {
                orderedSupply.Quantity = newQuantity;
                UpdateTotal();
            };

            return detail;
        }


        private async void Click_BtnConfirmOrder(object sender, RoutedEventArgs e)
        {
            await RegisterSupplierOrder();
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
