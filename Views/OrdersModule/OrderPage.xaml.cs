using System;
using System.Collections.Generic;
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
using System.IO;
using System.Printing;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
        }

        private void Btn_OrderPay(object sender, RoutedEventArgs e)
        {
            // 1. Crear la vista del ticket
            TicketPage ticketPage = new TicketPage();
            ticketPage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            ticketPage.Arrange(new Rect(ticketPage.DesiredSize));

            // 2. Renderizar la vista en un Visual
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)ticketPage.ActualWidth, (int)ticketPage.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(ticketPage);

            // 3. Guardar la imagen en memoria
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                // 4. Crear el documento PDF
                using (FileStream fs = new FileStream("Ticket.pdf", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Document doc = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // 5. Convertir la imagen a iTextSharp
                    iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(imageBytes);
                    pdfImage.ScaleToFit(500, 800); // Ajustar tamaño de la imagen en el PDF
                    doc.Add(pdfImage);

                    doc.Close();
                }
            }

            MessageBox.Show("Ticket exportado como PDF.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
