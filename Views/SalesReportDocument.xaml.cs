using ItaliaPizzaClient.ItaliaPizzaServices;
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

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para SalesReportDocument.xaml
    /// </summary>
    public partial class SalesReportDocument : Page
    {
        public SalesReportDocument(IEnumerable<CashRegisterDTO> data, DateTime start, DateTime end)
        {
            InitializeComponent();

            DateRangeText.Text = $"From {start:dd/MM/yyyy} to {end:dd/MM/yyyy}";
            PrintDataGrid.ItemsSource = data;
        }
    }
}
