using ItaliaPizzaClient.Model;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterSupplyPage.xaml
    /// </summary>
    public partial class RegisterSupplyPage : Page
    {
        public RegisterSupplyPage()
        {
            InitializeComponent();

            CbCategories.ItemsSource = SupplyCategory.GetDefaultSupplyCategories();
            CbUnitMeasures.ItemsSource = UnitMeasure.GetDefaultUnitMeasures();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
