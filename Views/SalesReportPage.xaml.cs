using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
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
    public partial class SalesReportPage : Page
    {
        private List<CashRegisterDTO> _reportData = new List<CashRegisterDTO>();
        public SalesReportPage()
        {
            InitializeComponent();
        }

        private async void LoadSalesReportData()
        {
            if (!startDate.SelectedDate.HasValue || !endDate.SelectedDate.HasValue)
                return;

            DateTime start = startDate.SelectedDate.Value.Date;
            DateTime end = endDate.SelectedDate.Value.Date;

            if (start > end) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetCashRegisterByDate(start, end);

                var reportList = dtoList.Select(c => new CashRegisterDTO
                {
                    CashRegisterID = c.CashRegisterID,
                    InitialBalance = c.InitialBalance,
                    OpeningDate = c.OpeningDate,
                    CashierAmount = c.CashierAmount
                }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ReportDetailsDataGrid.ItemsSource = reportList;
                });
            });
        }

        private void startDate_CalendarOpened(object sender, RoutedEventArgs e)
        {
            if (startDate.SelectedDate.HasValue)
                startDate.DisplayDate = startDate.SelectedDate.Value;
        }

        private void endDate_CalendarOpened(object sender, RoutedEventArgs e)
        {
            if (endDate.SelectedDate.HasValue)
                endDate.DisplayDate = endDate.SelectedDate.Value;
        }

        private void startDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (startDate.SelectedDate > DateTime.Today)
            {
                MessageDialog.Show("SalesReport_DialogTToday", "SalesReport_DialogDToday", AlertType.WARNING);
                startDate.SelectedDate = DateTime.Today;
                return;
            }
            if (endDate.SelectedDate.HasValue && startDate.SelectedDate > endDate.SelectedDate)
            {
                MessageDialog.Show("SalesReport_DialogTEndStart", "SalesReport_DialogDEndStart", AlertType.WARNING);
                startDate.SelectedDate = endDate.SelectedDate;
                return;
            }

            LoadSalesReportData();
        }

        private void endDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (endDate.SelectedDate > DateTime.Today)
            {
                MessageDialog.Show("SalesReport_DialogTToday", "SalesReport_DialogDToday", AlertType.WARNING);
                endDate.SelectedDate = DateTime.Today;
                return;
            }
            if (startDate.SelectedDate.HasValue && endDate.SelectedDate > startDate.SelectedDate)
            {
                MessageDialog.Show("SalesReport_DialogTEndStart", "SalesReport_DialogDEndStart", AlertType.WARNING);
                endDate.SelectedDate = startDate.SelectedDate;
                return;
            }

            LoadSalesReportData();
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            /*
            var data = ReportDetailsDataGrid.ItemsSource as IEnumerable<CashRegisterDTO>;
            if (data == null || !data.Any())
            {
                MessageDialog.Show("SalesReport_DialogTNoRecords", "SalesReport_DialogDNoRecords", AlertType.WARNING);
                return;
            }

            var printPage = new SalesReportDocument(data, startDate.SelectedDate.Value, endDate.SelectedDate.Value);

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(printPage, "Sales Report");
            }
            */
        }
    }
}
