using ItaliaPizzaClient.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.UserControls;

namespace ItaliaPizzaClient.Views
{
    public partial class OrderSuppliersPage : Page
    {
        private List<SupplierOrder> _supplierOrders = new List<SupplierOrder>();

        public OrderSuppliersPage()
        {
            InitializeComponent();
            Loaded += OrderSuppliersPage_Loaded;
        }

        private void OrderSuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
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
                    SupplierID = dto.SupplierID,
                    SupplierName = dto.SupplierName,
                    OrderedDate = dto.OrderedDate,
                    OrderFolio = dto.OrderFolio,
                    Delivered = dto.Delivered,
                    Total = dto.Total,
                    OrderedDateFormatted = dto.OrderedDate.ToString("dd/MM/yyyy"),
                    Status = dto.Status,
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

                _supplierOrders = orders;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SupplierOrdersDataGrid.ItemsSource = _supplierOrders;
                });
            });
        }

        private void DisplaySupplyDetails(SupplierOrder selected)
        {
            if (selected == null) return;

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
        }

        private void Click_BtnNewOrderSupplier(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
                mainWindow.NavigateToPage("RegOrderSupplier_Header", new RegisterOrderSupplierPage());
        }

        private void Click_BtnCancelOrder(object sender, RoutedEventArgs e)
        {

        }

        private void SupplierOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (SupplierOrder) SupplierOrdersDataGrid.SelectedItem;

            if (selected != null)
            {
                DisplaySupplyDetails(selected);
                ShowOrderDetails(selected.Items);
            }
        }
    }
}
