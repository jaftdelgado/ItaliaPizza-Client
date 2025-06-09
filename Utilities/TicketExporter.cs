using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using ItaliaPizzaClient.Views;

namespace ItaliaPizzaClient.Utilities
{
    public static class TicketExporter
    {
        public static void ExportTicketToPDF(
            int orderId,
            decimal total,
            decimal cash,
            decimal change,
            List<string> productNames,
            string outputPath = null)
        {
            var ticketPage = new TicketPage();

            // Asignar valores a los campos visuales
            ticketPage.ProductDataGrid.ItemsSource = productNames.ConvertAll(name => new { Product = name, Quantity = 1 });

            ticketPage.TxtTotalItems.Text = $"{productNames.Count}";
            ticketPage.txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ticketPage.TxtTotal.Text = $"{total:0.00}";
            ticketPage.TxtCash.Text = $"{cash:0.00}";
            ticketPage.TxtChange.Text = $"{change:0.00}";

            // Medir y renderizar el layout
            ticketPage.Measure(new Size(320, 1000));
            ticketPage.Arrange(new Rect(new Size(320, 1000)));
            ticketPage.UpdateLayout();

            // Forzar generación de filas
            foreach (var item in ticketPage.ProductDataGrid.Items)
            {
                ticketPage.ProductDataGrid.ScrollIntoView(item);
                ticketPage.ProductDataGrid.UpdateLayout();
            }

            ticketPage.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            var bitmap = new RenderTargetBitmap(320, 1000, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(ticketPage);

            // Convertir a PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            byte[] imageData;
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                imageData = ms.ToArray();
            }

            // Mostrar diálogo para guardar archivo si no se proporciona una ruta
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Guardar ticket como PDF",
                    Filter = "Archivos PDF (*.pdf)|*.pdf",
                    FileName = $"Ticket_{orderId}.pdf",
                    DefaultExt = ".pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    outputPath = dialog.FileName;
                }
                else
                {
                    // Canceló el diálogo
                    return;
                }
            }

            // Exportar a PDF
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
    }
}
