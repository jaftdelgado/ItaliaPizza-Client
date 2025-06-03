using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views.OrdersModule
{
    public partial class OrdersPage : Page
    {
        private List<Order> _allOrders = new List<Order>();

        public OrdersPage()
        {
            InitializeComponent();

            Loaded += OrdersPage_Loaded;
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            var statusFilter = new int[] { 0, 1, 2 };
            bool includeLocal = true;
            bool includeDelivery = true;

            await LoadOrdersData(statusFilter, includeLocal, includeDelivery);
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

        private void Click_BtnNewOrder(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
                mainWindow.NavigateToPage("RegOrder_Header", new RegisterOrderPage());
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnEditOrder(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnCancelOrder(object sender, RoutedEventArgs e)
        {

        }
    }
}
