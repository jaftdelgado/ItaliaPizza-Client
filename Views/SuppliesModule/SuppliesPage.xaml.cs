using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para SuppliesPage.xaml
    /// </summary>
    public partial class SuppliesPage : Page
    {
        public SuppliesPage()
        {
            InitializeComponent();
            Loaded += SupplyPage_Loaded;
        }

        private void SupplyPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliesData();
        }

        private async void LoadSuppliesData()
        {
            await ConnectionUtilities.ExecuteServerAction(async () =>
            {
                var client = ConnectionUtilities.IsServerConnected();
                if (client == null)
                    return;

                var dtoList = client.GetAllSupplies();

                var list = dtoList.Select(s => new Supply
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    MeasureUnit = s.MeasureUnit,
                    Brand = s.Brand,
                    SupplyCategoryID = s.SupplyCategoryID,
                    SupplierID = s.SupplierID
                })
                .ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SupplyDataGrid.ItemsSource = list;
                });
            });
        }


        private void Click_BtnNewSupply(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegSupply_Header", new RegisterSupplyPage());
        }

        private void Click_BtnEditSupply(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnDeleteSupply(object sender, RoutedEventArgs e)
        {

        }
    }
}
