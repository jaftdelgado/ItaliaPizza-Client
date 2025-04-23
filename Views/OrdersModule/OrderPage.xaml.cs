using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ItaliaPizzaClient.Views.OrdersModule
{
    /// <summary>
    /// Lógica de interacción para OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private ObservableCollection<OrderSummaryDTO> deliveredOrders;

        public OrderPage()
        {
            InitializeComponent();
            LoadDeliveredOrders();
        }
        private void LoadDeliveredOrders()
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            try
            {
                var orders = client.GetDeliveredOrders();
                deliveredOrders = new ObservableCollection<OrderSummaryDTO>(orders);
                OrdersDataGrid.ItemsSource = deliveredOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar órdenes: " + ex.Message);
            }
        }
        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            if (OrdersDataGrid.SelectedItem is OrderSummaryDTO selectedOrder)
            {
                try
                {
                    var items = client.GetOrderItemsByOrderID(selectedOrder.OrderID);
                    DetailsOrdersDataGrid.ItemsSource = items;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al obtener detalles de la orden: " + ex.Message);
                }
            }
        }

        private void Clic_BtnPayOrder(object sender, RoutedEventArgs e)
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            if (OrdersDataGrid.SelectedItem is OrderSummaryDTO selectedOrder)
            {
                try
                {
                    bool success = client.RegisterOrderPayment(selectedOrder.OrderID);

                    if (success)
                    {
                        MessageDialog.Show("Orden pagada exitosamente", "Éxito", AlertType.SUCCESS);
                        LoadDeliveredOrders(); 
                        DetailsOrdersDataGrid.ItemsSource = null;
                        var items = client.GetOrderItemsByOrderID(selectedOrder.OrderID).ToList();
                        ExportTicketToPDF(selectedOrder, items);

                    }
                    else
                    {
                        MessageDialog.Show("No se pudo pagar la orden. Verifique que esté en estado entregada y que exista una caja abierta.", "Error", AlertType.ERROR);
                    }
                }
                catch (Exception ex)
                {
                    MessageDialog.Show("Error al procesar el pago: " + ex.Message, "Error", AlertType.ERROR);
                }
            }
            else
            {
                MessageDialog.Show("Seleccione una orden antes de pagar.", "Advertencia", AlertType.WARNING);
            }
        }
        private void ExportTicketToPDF(OrderSummaryDTO order, List<OrderItemSummaryDTO> items)
        {
            var ticketPage = new TicketPage();

            ticketPage.ProductDataGrid.ItemsSource = items;
            ticketPage.TxtTotalItems.Text = $"TOTAL                            {items.Count}";
            ticketPage.txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ticketPage.TxtTotal.Text = $"TOTAL:                         {order.Total:0.00}";

            ticketPage.Measure(new Size(320, 1000));
            ticketPage.Arrange(new Rect(new Size(320, 1000)));
            ticketPage.UpdateLayout();

            foreach (var item in ticketPage.ProductDataGrid.Items)
            {
                var row = (DataGridRow)ticketPage.ProductDataGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (row == null)
                {
                    ticketPage.ProductDataGrid.ScrollIntoView(item);
                    ticketPage.ProductDataGrid.UpdateLayout();
                    row = (DataGridRow)ticketPage.ProductDataGrid.ItemContainerGenerator.ContainerFromItem(item);
                }

                if (row != null)
                    row.UpdateLayout();
            }

            ticketPage.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            var bitmap = new RenderTargetBitmap(320, 1000, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(ticketPage);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                imageData = ms.ToArray();
            }

            string outputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Ticket_{order.OrderID}.pdf");

            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                using (Document doc = new Document(PageSize.A4))
                {
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    var ticketImage = iTextSharp.text.Image.GetInstance(imageData);
                    ticketImage.Alignment = Element.ALIGN_CENTER;
                    ticketImage.ScaleToFit(300f, 800f);
                    doc.Add(ticketImage);

                    doc.Close();
                }
            }
        }
        private void Clic_BtnNewOrder(object sender, RoutedEventArgs e)
        {

        }

        private void Clic_BtnCancelOrder(object sender, RoutedEventArgs e)
        {

        }
    }
}
