using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.UserControls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Linq;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterOrderSupplierPage1 : Page
    {
        public ObservableCollection<SupplyCard> SupplyCards { get; set; } = new ObservableCollection<SupplyCard>();
        private ObservableCollection<Supplier> _suppliers = new ObservableCollection<Supplier>();

        public RegisterOrderSupplierPage1()
        {
            InitializeComponent();
            SetCategoriesComboBox();
        }

        private void SetCategoriesComboBox()
        {
            CbCategories.ItemsSource = SupplyCategory.GetDefaultSupplyCategories();
        }

        private async void LoadSuppliers()
        {
            var selectedCategory = CbCategories.SelectedItem as SupplyCategory;
            if (selectedCategory == null) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetSuppliersByCategory(selectedCategory.Id);

                var suppliers = dtoList.Select(s => new Supplier
                {
                    Id = s.Id,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    PhoneNumber = s.PhoneNumber,
                    EmailAddress = s.EmailAddress,
                    Description = s.Description,
                    CategorySupply = s.CategorySupply,
                    IsActive = s.IsActive
                })
                .Where(s => s.IsActive).OrderBy(s => s.SupplierName).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _suppliers.Clear();
                    foreach (var supplier in suppliers)
                        _suppliers.Add(supplier);

                    CbSuppliers.ItemsSource = _suppliers.ToList();
                });
            });
        }

        private async void LoadSuppliesForSelectedSupplier()
        {
            var selectedSupplier = CbSuppliers.SelectedItem as Supplier;
            if (selectedSupplier == null) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetSuppliesBySupplier(selectedSupplier.Id);

                var supplies = dtoList.Select(s => new Supply
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    MeasureUnit = s.MeasureUnit,
                    Brand = s.Brand,
                    SupplyPic = s.SupplyPic,
                    SupplierID = s.SupplierID
                }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SupplyCards.Clear();
                    foreach (var supply in supplies)
                    {
                        var card = CreateSupplyCard(supply);
                        SupplyCards.Add(card);
                    }

                    ButtonsPanel.ItemsSource = SupplyCards;
                });
            });
        }

        private SupplyCard CreateSupplyCard(Supply supply)
        {
            var card = new SupplyCard
            {
                SupplyName = supply.Name,
                StockText = $"Stock: {supply.MeasureUnit}",
                PriceText = supply.FormattedPricePerUnit,
                Margin = new Thickness(0, 0, 10, 10)
            };

            ImageUtilities.SetImageSource(card.SupplyPic, supply.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

            return card;
        }

        private void Click_BtnConfirmOrder(object sender, RoutedEventArgs e)
        {

        }

        private void CbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SupplyCards.Clear();
            LoadSuppliers();
        }

        private void CbSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SupplyCards.Clear();
            LoadSuppliesForSelectedSupplier();
        }
    }
}
