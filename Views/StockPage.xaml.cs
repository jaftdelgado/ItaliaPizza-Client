using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class StockPage : Page
    {
        private List<Supply> _allSupplies = new List<Supply>();

        public StockPage()
        {
            InitializeComponent();
            Loaded += StockPage_Loaded;
        }

        private void StockPage_Loaded(object sender, RoutedEventArgs e)
        {
            //UpdateStockPanelVisibility(null);
            LoadSuppliesData();
        }

        private async void LoadSuppliesData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllSupplies(true);

                var list = dtoList.Select(s => new Supply
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    MeasureUnit = s.MeasureUnit,
                    Brand = s.Brand,
                    SupplyPic = s.SupplyPic,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    Stock = s.Stock,
                    SupplyCategoryID = s.SupplyCategoryID,
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName,
                    CanBeDeleted = s.IsDeletable
                })
                .OrderBy(p => p.SupplyCategoryID)
                .ToList();

                _allSupplies = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SupplyDataGrid.ItemsSource = _allSupplies;
                });
            });

        }
    }
}