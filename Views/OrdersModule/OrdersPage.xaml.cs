using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;

namespace ItaliaPizzaClient.Views.OrdersModule
{
    public partial class OrdersPage : Page
    {
        private List<Order> _allOrders = new List<Order>();

        private int CurrentUserRoleId = CurrentSession.LoggedInUser.RoleID;

        public OrdersPage()
        {
            InitializeComponent();

            Loaded += OrdersPage_Loaded;

            if(CurrentUserRoleId == 5) BtnNewOrder.Visibility = Visibility.Collapsed; 
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshOrders();
        }

        private async Task RefreshOrders()
        {
            int[] statusFilter;
            bool includeLocal, includeDelivery;

            GetOrderFiltersForRole(CurrentUserRoleId, out statusFilter, out includeLocal, out includeDelivery);

            await LoadOrdersData(statusFilter, includeLocal, includeDelivery);
        }

        private void GetOrderFiltersForRole(int roleId, out int[] statusFilter, out bool includeLocal, out bool includeDelivery)
        {
            switch (roleId)
            {
                case 3: statusFilter = null; includeLocal = true; includeDelivery = true; break;                    // Cashier
                case 4: statusFilter = new[] { 1, 2, 3, 5 }; includeLocal = true; includeDelivery = false; break;   // Waiter
                case 5: statusFilter = new[] { 1, 2, 3 }; includeLocal = true; includeDelivery = true; break;       // Cook
                default: statusFilter = new int[0]; includeLocal = false; includeDelivery = false; break;
            }
        }

        private async Task LoadOrdersData(int[] statusFilter, bool includeLocal, bool includeDelivery)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                List<OrderDTO> dtoList = (await Task.Run(() => client.GetOrders(statusFilter, includeLocal, includeDelivery))).ToList();

                var orders = dtoList.Select(dto => new Order
                {
                    OrderID = dto.OrderID,
                    CustomerID = dto.CustomerID,
                    OrderDate = dto.OrderDate,
                    Total = dto.Total,
                    IsDelivery = dto.IsDelivery,
                    PersonalID = dto.PersonalID,
                    AttendedByName = dto.AttendedByName,
                    DeliveryID = dto.DeliveryID,
                    Status = dto.Status,
                    TableNumber = dto.TableNumber,
                    Items = dto.Items?.Select(item => new OrderedProduct
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity ?? 0,
                        Name = item.Name,
                        Price = item.Price,
                        ProductPic = item.ProductPic
                    }).ToList(),
                    DeliveryInfo = dto.DeliveryInfo == null ? null : new Delivery
                    {
                        DeliveryID = dto.DeliveryInfo.DeliveryID,
                        AddressID = dto.DeliveryInfo.AddressID,
                        DeliveryDriverID = dto.DeliveryInfo.DeliveryDriverID,
                        CustomerFullName = dto.DeliveryInfo.CustomerFullName,
                        CustomerAddress = dto.DeliveryInfo.CustomerAddress,
                        DeliveryDriverName = dto.DeliveryInfo.DeliveryDriverName
                    }
                }).ToList();

                _allOrders = orders;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    OrdersDataGrid.ItemsSource = _allOrders;
                });
            });
        }

        private async Task<bool> ChangeOrderStatus(int orderId, int newStatus)
        {
            bool result = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null)
                {
                    result = false;
                    return;
                }

                await Task.Run(() => client.ChangeOrderStatus(orderId, newStatus, CurrentUserRoleId));

                result = true;
            });

            return result;
        }


        #region Role-Specific ChangeStatus
        private async Task ChangeOrderStatusWaiterRole(Order order, int newStatus)
        {
            if (order == null) return;

            var allowedStatuses = new HashSet<int> { 0, 5 };

            if (!allowedStatuses.Contains(newStatus)) return;

            string titleKey = "";
            string descriptionKey = "";
            AlertType alertType = AlertType.SUCCESS;

            switch (newStatus)
            {
                case 0: // Cancelar
                    MessageDialog.ShowConfirm(
                        "Waiter_DialogTitle_ConfirmCancel",
                        "Waiter_DialogDesc_ConfirmCancel",
                        async () =>
                        {
                            bool success = await ChangeOrderStatus(order.OrderID, newStatus);
                            if (success)
                            {
                                MessageDialog.Show("Waiter_DialogTitle_Cancelled", "Waiter_DialogDesc_Cancelled", AlertType.SUCCESS);
                                await RefreshOrders();
                            }
                        },
                        "Glb_Cancel"
                    );
                    return;

                case 5: // Marcar como entregado
                    titleKey = "Waiter_DialogTitle_Delivered";
                    descriptionKey = "Waiter_DialogDesc_Delivered";
                    break;
            }

            bool changed = await ChangeOrderStatus(order.OrderID, newStatus);
            if (changed)
            {
                MessageDialog.Show(titleKey, descriptionKey, alertType);
                await RefreshOrders();
            }
        }

        private async Task ChangeOrderStatusCashierRole(Order order, int newStatus)
        {
            if (order == null) return;

            var allowedStatuses = new HashSet<int> { 0, 4, 5, 7 };

            if (!allowedStatuses.Contains(newStatus)) return;

            string titleKey = "";
            string descriptionKey = "";
            AlertType alertType = AlertType.SUCCESS;

            switch (newStatus)
            {
                case 0: // Cancelar
                    MessageDialog.ShowConfirm(
                        "Cashier_DialogTitle_ConfirmCancel",
                        "Cashier_DialogDesc_ConfirmCancel",
                        async () =>
                        {
                            bool success = await ChangeOrderStatus(order.OrderID, newStatus);
                            if (success)
                            {
                                MessageDialog.Show("Cashier_DialogTitle_Cancelled", "Cashier_DialogDesc_Cancelled", AlertType.SUCCESS);
                                await RefreshOrders();
                            }
                        },
                        "Glb_Cancel"
                    );
                    return;

                case 4: // Marcar como enviado (solo si es a domicilio)
                    titleKey = "Cashier_DialogTitle_Sent";
                    descriptionKey = "Cashier_DialogDesc_Sent";
                    break;

                case 5: // Marcar como entregado (en sucursal)
                    titleKey = "Cashier_DialogTitle_Delivered";
                    descriptionKey = "Cashier_DialogDesc_Delivered";
                    break;

                case 7: // Marcar como no entregado
                    titleKey = "Cashier_DialogTitle_NotDelivered";
                    descriptionKey = "Cashier_DialogDesc_NotDelivered";
                    break;
            }

            bool changed = await ChangeOrderStatus(order.OrderID, newStatus);
            if (changed)
            {
                MessageDialog.Show(titleKey, descriptionKey, alertType);
                await RefreshOrders();
            }
        }

        private async Task ChangeOrderStatusCookRole(Order order, int newStatus)
        {
            if (order == null) return;

            var allowedStatuses = new HashSet<int> { 0, 2, 3 };

            if (!allowedStatuses.Contains(newStatus)) return;

            string titleKey = "";
            string descriptionKey = "";
            AlertType alertType = AlertType.SUCCESS;

            switch (newStatus)
            {
                case 0:
                    MessageDialog.ShowConfirm(
                        "Kitchen_DialogTitle_ConfirmCancel",
                        "Kitchen_DialogDesc_ConfirmCancel",
                        async () =>
                        {
                            bool success = await ChangeOrderStatus(order.OrderID, newStatus);
                            if (success)
                            {
                                MessageDialog.Show("Kitchen_DialogTitle_Cancelled", "Kitchen_DialogDesc_Cancelled", AlertType.SUCCESS);
                                await RefreshOrders();
                            }
                        },
                        "Glb_Cancel"
                    );
                    return;

                case 2:
                    titleKey = "Kitchen_DialogTitle_Preparing";
                    descriptionKey = "Kitchen_DialogDesc_Preparing";
                    break;

                case 3:
                    titleKey = "Kitchen_DialogTitle_Ready";
                    descriptionKey = "Kitchen_DialogDesc_Ready";
                    break;
            }

            bool changed = await ChangeOrderStatus(order.OrderID, newStatus);
            if (changed)
            {
                MessageDialog.Show(titleKey, descriptionKey, alertType);
                await RefreshOrders();
            }
        }
        #endregion

        private void DisplayOrderDetails(Order selected)
        {
            if (selected == null) return;

            UpdateOrderPanelVisibility(selected);
            ShowOrderedProducts(selected.Items);

            OrderNum.Text = selected.FormattedOrderID;
            OrderedDate.Text = selected.OrderDateFormatted;

            if (selected.IsDelivery == false)
            {
                TablePanel.Visibility = Visibility.Visible;
                TableNumber.Text = selected.TableNumber;
            } 
            else TablePanel.Visibility = Visibility.Collapsed;

            UpdateButtonsPanelVisibility(CurrentUserRoleId, selected.Status);
        }

        private void ShowOrderedProducts(List<OrderedProduct> orderedProducts)
        {
            OrderedProductsDetailsPanel.Children.Clear();

            foreach (var item in orderedProducts)
            {
                var detail = new ProductOrderDetail
                {
                    ProductName = item.Name,
                    Price = item.Price ?? 0,
                    Quantity = item.Quantity,
                    Margin = new Thickness(0, 0, 0, 6),
                    IsReadOnly = true
                };

                detail.Subtotal = item.TotalPrice;

                ImageUtilities.SetImageSource(detail.ProductPic, item.ProductPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

                OrderedProductsDetailsPanel.Children.Add(detail);
            }
        }

        private void UpdateButtonsPanelVisibility(int roleId, int orderStatus)
        {
            CookButtonsPanel.Visibility = Visibility.Collapsed;
            WaiterButtonsPanel.Visibility = Visibility.Collapsed;
            CashierButtonsPanel.Visibility = Visibility.Collapsed;

            CookTakenButtons.Visibility = Visibility.Collapsed;
            CookPreparingButtons.Visibility = Visibility.Collapsed;
            WaiterTakenButtons.Visibility = Visibility.Collapsed;
            WaiterPreparedButtons.Visibility = Visibility.Collapsed;

            CashierTakenButtons.Visibility = Visibility.Collapsed;
            CashierPreparedButtons.Visibility = Visibility.Collapsed;
            CashierDeliveredButtons.Visibility = Visibility.Collapsed;
            PaymentPanel.Visibility = Visibility.Collapsed;

            //ROL: Cook
            if (roleId == 5)
            {
                CookButtonsPanel.Visibility = Visibility.Visible;

                switch (orderStatus)
                {
                    case 1:
                        CookTakenButtons.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        CookPreparingButtons.Visibility = Visibility.Visible;
                        break;
                    default:
                        CookButtonsPanel.Visibility = Visibility.Collapsed;
                        break;
                }
            }

            //ROL: Waiter
            if (roleId == 4)
            {
                WaiterButtonsPanel.Visibility = Visibility.Visible;

                switch (orderStatus)
                {
                    case 1:
                        WaiterTakenButtons.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        WaiterPreparedButtons.Visibility = Visibility.Visible;
                        break;
                    default:
                        WaiterButtonsPanel.Visibility = Visibility.Collapsed;
                        break;
                }
            }

            //ROL: Cashier
            if (roleId == 3 && OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                CashierButtonsPanel.Visibility = Visibility.Visible;

                switch (orderStatus)
                {
                    case 1:
                        CashierTakenButtons.Visibility = Visibility.Visible;
                        break;

                    case 3:
                        if (selectedOrder.IsDelivery == true)
                            CashierPreparedButtons.Visibility = Visibility.Visible;
                        
                        else
                        {
                            PaymentPanel.Visibility = Visibility.Visible;
                            TbOrderTotal.Text = $"${selectedOrder.Total:F2}";
                        }
                        break;

                    case 4:
                        CashierDeliveredButtons.Visibility = Visibility.Visible;
                        break;

                    case 5:
                        PaymentCashier.Visibility = Visibility.Visible;
                        break;

                    default:
                        CashierButtonsPanel.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void UpdateOrderPanelVisibility(Order selected)
        {
            bool hasSelection = selected != null;

            OrderDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Click_BtnNewOrder(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
                mainWindow.NavigateToPage("RegOrder_Header", new RegisterOrderPage());
        }

        #region ChangeStatus Buttons
        private async void Click_BtnMarkReady(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCookRole(selected, 3);
        }

        private async void Click_BtnCancelOrder(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCookRole(selected, 0);
        }

        private async void Click_BtnStartPreparing(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCookRole(selected, 2);
        }

        private async void Click_BtnWaiterCancelOrder(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusWaiterRole(selected, 0);
        }

        private async void Click_BtnMarkDelivered(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusWaiterRole(selected, 5);
        }

        private async void Click_BtnCashierCancelOrder(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCashierRole(selected, 0);
        }

        private async void Click_BtnMarkAsShipped(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCashierRole(selected, 4);
        }

        private async void Click_BtnCashierMarkDelivered(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCashierRole(selected, 5);
        }

        private async void Click_BtnMarkNotDelivered(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
                await ChangeOrderStatusCashierRole(selected, 7);
        }

        #endregion

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnEditOrder(object sender, RoutedEventArgs e)
        {

        }

        private void SupplierOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected) DisplayOrderDetails(selected);
            
            else UpdateOrderPanelVisibility(null);
        }

        private void Click_BtnPayOrder(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selected)
            {
                PaymentCashier.Visibility = Visibility.Collapsed;
                PaymentPanel.Visibility = Visibility.Visible;
                TbOrderTotal.Text = $"${selected.Total:F2}";
            }
        }
        
        private void Click_BtnCancelPayment(object sender, RoutedEventArgs e)
        {
            PaymentPanel.Visibility = Visibility.Collapsed;
            PaymentCashier.Visibility = Visibility.Visible;
        }
    }
}
