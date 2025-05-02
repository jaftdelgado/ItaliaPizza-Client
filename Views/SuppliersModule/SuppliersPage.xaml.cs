using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.SuppliersModule
{
    public partial class SuppliersPage : Page
    {
        private List<Supplier> _allSuppliers = new List<Supplier>();

        public SuppliersPage()
        {
            InitializeComponent();
            Loaded += SuppliersPage_Loaded;
        }

        private async void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSupplierData();
        }

        private async Task LoadSupplierData()
        {
            await ConnectionUtilities.ExecuteServerAction(async () =>
            {
                var client = ConnectionUtilities.IsServerConnected();
                if (client == null)
                    return;

                var dtoList = client.GetAllSuppliers();

                var list = dtoList.Select(s => new Supplier
                {
                    Id = s.Id,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    PhoneNumber = s.PhoneNumber,
                    EmailAddress = s.EmailAddress,
                    Description = s.Description,
                    CategorySupply = s.CategorySupply
                })
                .OrderBy(s => s.SupplierName)
                .ToList();

                _allSuppliers = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SuppliersDataGrid.ItemsSource = _allSuppliers;
                });
            });
        }

        private void Click_BtnNewSupplier(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
                mainWindow.NavigateToPage("RegSupplier_Header", new RegisterSupplierPage());
        }
    }
}
