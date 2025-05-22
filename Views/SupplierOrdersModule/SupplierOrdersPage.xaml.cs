using ItaliaPizzaClient.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.UserControls;
using ItaliaPizzaClient.Views.Dialogs;
using System.Threading.Tasks;
using System;

namespace ItaliaPizzaClient.Views.SupplierOrdersModule
{
    public partial class SupplierOrdersPage : Page
    {
        private List<SupplierOrder> _allSupplierOrders = new List<SupplierOrder>();

        public SupplierOrdersPage()
        {
            InitializeComponent();
            BtnPending.Tag = "Selected";
            Loaded += OrderSuppliersPage_Loaded;
            InputUtilities.ValidatePriceInput(TbPayment, @"^\d{0,5}(\.\d{0,2})?$", 99999.999m);
        }

        private void OrderSuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            SupplierOrderDetailsPanel.Visibility = Visibility.Collapsed;
            LoadSupplierOrdersData();
        }

        private void ShowOrderDetails(List<OrderedSupply> orderedSupplies)
        {
            OrderDetailsPanel.Children.Clear();

            foreach (var item in orderedSupplies)
            {
                var detail = new SupplyOrderDetail
                {
                    SupplyName = item.SupplyName,
                    Price = item.UnitPrice,
                    Quantity = item.Quantity,
                    MeasureUnitId = item.Supply?.MeasureUnit ?? 0,
                    Margin = new Thickness(0, 0, 0, 6),
                    IsReadOnly = true
                };

                detail.Subtotal = item.TotalPrice;
                detail.Unit = item.Unit;

                ImageUtilities.SetImageSource(detail.SupplyPic, item.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

                OrderDetailsPanel.Children.Add(detail);
            }
        }

        private async void LoadSupplierOrdersData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllSupplierOrders();

                var orders = dtoList.Select(dto => new SupplierOrder
                {
                    SupplierOrderID = dto.SupplierOrderID,
                    SupplierID = dto.SupplierID,
                    SupplierName = dto.SupplierName,
                    OrderedDate = dto.OrderedDate,
                    OrderFolio = dto.OrderFolio,
                    Delivered = dto.Delivered,
                    Total = dto.Total,
                    OrderedDateFormatted = dto.OrderedDate.ToString("dd/MM/yyyy"),
                    Status = dto.Status,
                    CategorySupplyID = dto.CategorySupplyID,
                    Items = dto.Items.Select(item => new OrderedSupply
                    {
                        Quantity = item.Quantity,
                        Supply = new Supply
                        {
                            Id = item.SupplyID,
                            Name = item.SupplyName,
                            SupplyPic = item.SupplyPic,
                            Price = item.UnitPrice,
                            MeasureUnit = item.MeasureUnit
                        }
                    }).ToList()
                }).ToList();

                _allSupplierOrders = orders;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter("BtnPending");
                });
            });
        }
        private async Task DeliverSupplierOrder(SupplierOrder selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                int result = await client.RegisterSupplierOrderExpenseAsync(selected.SupplierOrderID);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (result == 1)
                    {
                        bool success = client.DeliverOrder(selected.SupplierOrderID);
                        if (!success)
                        {
                            MessageDialog.Show("OrdSuppliers_ErrorDelivery_Title", "OrdSuppliers_ErrorDelivery_Failed", AlertType.ERROR);
                            return;
                        }

                        var item = _allSupplierOrders.FirstOrDefault(p => p.SupplierOrderID == selected.SupplierOrderID);
                        if (item != null)
                        {
                            item.Status = 1;
                            item.Delivered = DateTime.Now;
                        }

                        MessageDialog.Show("OrdSuppliers_DialogTDeliveredOrder", "OrdSuppliers_DialogDDeliveredOrder", AlertType.SUCCESS, () =>
                        {
                            SupplierOrderDetailsPanel.Visibility = Visibility.Collapsed;
                            PaymentPanel.Visibility = Visibility.Collapsed;
                            OperationsPanel.Visibility = Visibility.Visible;
                            LoadSupplierOrdersData();
                        });
                    }
                    else
                    {
                        switch (result)
                        {
                            case -1:
                                MessageDialog.Show("OrdSuppliers_ErrorDelivery_Title", "OrdSuppliers_ErrorDelivery_InvalidStatus", AlertType.WARNING);
                                break;
                            case -2:
                                MessageDialog.Show("OrdSuppliers_ErrorDelivery_Title", "OrdSuppliers_ErrorDelivery_NoCashRegister", AlertType.WARNING);
                                break;
                            case -3:
                                MessageDialog.Show("OrdSuppliers_ErrorDelivery_Title", "OrdSuppliers_ErrorDelivery_InsufficientFunds", AlertType.WARNING);
                                break;
                            default:
                                MessageDialog.Show("OrdSuppliers_ErrorDelivery_Title", "OrdSuppliers_ErrorDelivery_TransactionFailed", AlertType.WARNING);
                                break;
                        }
                    }
                });
            });
        }
        private async Task CancelSupplierOrder(SupplierOrder selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool success = client.CancelSupplierOrder(selected.SupplierOrderID);
                if (!success) return;

                var item = _allSupplierOrders.FirstOrDefault(p => p.SupplierOrderID == selected.SupplierOrderID);
                if (item != null) item.Status = 2;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("OrdSuppliers_DialogTCancelledOrder", "OrdSuppliers_DialogDCancelledOrder", AlertType.SUCCESS);
                });
            });
        }

        private void SearchSupplierOrders()
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            string selectedFilter = GetSelectedFilterButtonName();

            IEnumerable<SupplierOrder> filteredList = _allSupplierOrders;

            switch (selectedFilter)
            {
                case "BtnPending":
                    filteredList = filteredList.Where(p => p.Status == 0);
                    break;
                case "BtnDelivered":
                    filteredList = filteredList.Where(p => p.Status == 1);
                    break;
                case "BtnCancelled":
                    filteredList = filteredList.Where(p => p.Status == 2);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredList = filteredList.Where(p =>
                    $"{p.OrderFolio}".ToLower().Contains(searchText) ||
                    p.SupplierName?.ToLower().Contains(searchText) == true ||
                    p.OrderedDate.ToString().Contains(searchText) == true ||
                    p.Delivered.ToString().Contains(searchText) == true 
                );
            }

            //EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                NoMatchesMessage.Visibility = Visibility.Visible;
            }

            SupplierOrdersDataGrid.ItemsSource = filteredList;
            UpdateElementsCounter(filteredList.Count());
        }

        private void ApplyFilter(string buttonName)
        {
            IEnumerable<SupplierOrder> filteredList = _allSupplierOrders;

            switch (buttonName)
            {
                case "BtnPending":
                    filteredList = _allSupplierOrders.Where(p => p.Status == 0);
                    break;
                case "BtnDelivered":
                    filteredList = _allSupplierOrders.Where(p => p.Status == 1);
                    break;
                case "BtnCancelled":
                    filteredList = _allSupplierOrders.Where(p => p.Status == 2);
                    break;
                case "BtnViewAll":
                    filteredList = _allSupplierOrders;
                    break;
            }

            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    //EmptyListMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    NoMatchesMessage.Visibility = Visibility.Visible;
                }
            }

            SupplierOrdersDataGrid.ItemsSource = filteredList;

            BtnPending.Tag = null;
            BtnDelivered.Tag = null;
            BtnCancelled.Tag = null;
            BtnViewAll.Tag = null;

            switch (buttonName)
            {
                case "BtnPending":
                    BtnPending.Tag = "Selected";
                    break;
                case "BtnDelivered":
                    BtnDelivered.Tag = "Selected";
                    break;
                case "BtnCancelled":
                    BtnCancelled.Tag = "Selected";
                    break;
                case "BtnViewAll":
                    BtnViewAll.Tag = "Selected";
                    break;
            }

            BtnDelivered.IsEnabled = _allSupplierOrders.Any(p => p.Status == 1);
            BtnCancelled.IsEnabled = _allSupplierOrders.Any(p => p.Status == 2);

            UpdateElementsCounter(filteredList.Count());
        }

        private void DisplayOrderDetails(SupplierOrder selected)
        {
            if (selected == null) return;

            UpdateOrderPanelVisibility(selected);
            OperationsPanel.Visibility = Visibility.Visible;
            PaymentPanel.Visibility = Visibility.Collapsed;

            SupplierName.Text = selected.SupplierName;
            OrderStatus.Text = selected.StatusDescription;
            OrderFolio.Text = selected.OrderFolio;
            OrderedDate.Text = selected.OrderedDate.ToString("dd/MM/yyyy");

            if (selected.Delivered.HasValue)
            {
                DeliveredPanel.Visibility = Visibility.Visible;
                DeliveredDate.Text = selected.Delivered.Value.ToString("dd/MM/yyyy");
            }
            else DeliveredPanel.Visibility = Visibility.Collapsed;

            TbOrderTotal.Text = selected.Total.ToString("C2");
            LblCurrentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            ButtonsPanel.Visibility = selected.Status == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnPending.Tag?.ToString() == "Selected") return "BtnPending";
            if (BtnDelivered.Tag?.ToString() == "Selected") return "BtnDelivered";
            if (BtnCancelled.Tag?.ToString() == "Selected") return "BtnCancelled";
            return "BtnViewAll";
        }

        private void UpdateOrderPanelVisibility(SupplierOrder selected)
        {
            bool hasSelection = selected != null;

            SupplierOrderDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateElementsCounter(int count)
        {
            ElementsCounter.Content = count.ToString();
        }

        private void Click_BtnNewOrderSupplier(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
                mainWindow.NavigateToPage("RegOrderSupplier_Header", new RegisterSupplierOrdersPage());
        }

        private void Click_BtnEditOrder(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            var orderToEdit = SupplierOrdersDataGrid.SelectedItem as SupplierOrder;

            if (mainWindow != null && orderToEdit != null)
                mainWindow.NavigateToPage("EditOrderSupplier_Header", new RegisterSupplierOrdersPage(orderToEdit));
        }

        private void Click_BtnDeliverOrder(object sender, RoutedEventArgs e)
        {
            OperationsPanel.Visibility = Visibility.Collapsed;
            PaymentPanel.Visibility = Visibility.Visible;
        }

        private void Click_BtnCancelOrder(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "OrdSuppliers_DialogTCancelOrder", "OrdSuppliers_DialogDCancelOrder",
                async () =>
                {
                    if (SupplierOrdersDataGrid.SelectedItem is SupplierOrder selected)
                        await CancelSupplierOrder(selected);
                },
                "Glb_Cancel"
            );
        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }

        private void SupplierOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SupplierOrdersDataGrid.SelectedItem is SupplierOrder selected)
            {
                DisplayOrderDetails(selected);
                ShowOrderDetails(selected.Items);
            }
            else
                UpdateOrderPanelVisibility(null);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchSupplierOrders();
        }

        private void TbPayment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TbOrderTotal.Text, System.Globalization.NumberStyles.Currency, null, out decimal total) &&
                decimal.TryParse(TbPayment.Text, System.Globalization.NumberStyles.Currency, null, out decimal efectivo))
            {
                decimal cambio = efectivo - total;
                TbChange.Text = cambio >= 0 ? cambio.ToString("C2") : string.Empty;
                BtnConfirm.IsEnabled = cambio >= 0;
            }
            
        }
        private void Clic_BtnConfirm(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "OrdSuppliers_DialogTConfirmDelivery",
                "OrdSuppliers_DialogDConfirmDelivery",  
                async () =>
                {
                    if (SupplierOrdersDataGrid.SelectedItem is SupplierOrder selected)
                    await DeliverSupplierOrder(selected);
                },
                "Glb_Confirm"
            );
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            PaymentPanel.Visibility = Visibility.Collapsed;
            OperationsPanel.Visibility = Visibility.Visible;
        }
    }
}
