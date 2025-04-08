using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ItaliaPizzaClient.ItaliaPizzaServices;
using System.Linq;
using ItaliaPizzaClient.Views.Dialogs;
using System;

namespace ItaliaPizzaClient.Views
{
    public partial class OrderPage : Page
    {
        private readonly MainManagerClient client = new MainManagerClient();
        private List<OrderSummaryDTO> ordersList = new List<OrderSummaryDTO>();

        public OrderPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            ordersList = client.GetDeliveredOrders().ToList();
            ordersDataGrid.ItemsSource = ordersList; 
        }



        private void ordersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ordersDataGrid.SelectedItem is OrderSummaryDTO selectedOrder)
            {
                OrdersDetailsDataGrid.ItemsSource = selectedOrder.Products.Select(i => new
                {
                    i.Product,
                    i.Quantity
                }).ToList();
            }
        }



        private void Btn_OrderPay(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ordersDataGrid.SelectedItem as OrderSummaryDTO;
            if (selectedOrder == null)
            {
                MessageDialog.Show("Validación", "Debes seleccionar una orden de la lista para pagar.", AlertType.WARNING);
                return;
            }


            var orderDto = new OrderDTO
            {
                OrderID = selectedOrder.OrderID,
                Total = selectedOrder.Total,
                Date = DateTime.Now
            };

            bool result = client.RegisterOrderPayment(orderDto);

            if (result)
            {
                MessageDialog.Show("Éxito", "Orden pagada correctamente.", AlertType.SUCCESS);
                LoadOrders();
                OrdersDetailsDataGrid.ItemsSource = null;
            }
            else
            {
                MessageDialog.Show("Error", "No se pudo completar el pago de la orden.", AlertType.ERROR);
            }
        }
    }
}
    

