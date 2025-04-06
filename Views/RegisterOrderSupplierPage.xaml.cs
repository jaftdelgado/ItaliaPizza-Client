using System.Windows.Controls;
using ItaliaPizzaClient.ItaliaPizzaServices;
using System.Windows;
using ItaliaPizzaClient.Model;
using System.Collections.Generic;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterOrderSupplierPage : Page
    {
        private readonly MainManagerClient client = new MainManagerClient();
        private readonly List<OrderSupplyItem> orderItems = new List<OrderSupplyItem>();


        public RegisterOrderSupplierPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = SupplyCategory.GetDefaultSupplyCategories();

            foreach (var category in categories)
            {
                category.Name = Application.Current.Resources[category.ResourceKey]?.ToString() ?? category.Name;
            }

            cbSuppliersCategories.ItemsSource = categories;
        }

        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOrderIfNeeded();
            if (cbSuppliersCategories.SelectedValue is string categoryName)
            {
                var suppliers = client.GetSuppliersByCategoryName(categoryName);

                cbSuppliersName.Items.Clear();

                foreach (var supplier in suppliers)
                {
                    cbSuppliersName.Items.Add(new ComboBoxItem
                    {
                        Content = supplier.ContactName, 
                        Tag = supplier                  
                    });
                }

                cbSuppliesName.ItemsSource = null;
            }
        }

        private void cbSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOrderIfNeeded();
            if (cbSuppliersName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplierDTO selectedSupplier)
            {
                int supplierId = selectedSupplier.SupplierID;

                var supplies = client.GetSuppliesBySupplier(supplierId);
                cbSuppliesName.Items.Clear();

                foreach (var supply in supplies)
                {
                    cbSuppliesName.Items.Add(new ComboBoxItem
                    {
                        Content = supply.SupplyName,
                        Tag = supply
                    });
                }
            }
        }
        private void AddSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbSuppliesName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplyDTO selectedSupply &&
                decimal.TryParse(txtQuantity.Text, out decimal quantity) &&
                quantity > 0)
            {
                orderItems.Add(new OrderSupplyItem
                {
                    SupplyName = selectedSupply.SupplyName,
                    Quantity = quantity,
                    MeasureUnit = selectedSupply.MeasureUnit
                });


                OrdersuppliersDataGrid.ItemsSource = null;
                OrdersuppliersDataGrid.ItemsSource = orderItems;

                // Limpiar campos
                cbSuppliesName.SelectedIndex = -1;
                txtQuantity.Clear();
            }
            else
            {
                //MessageBox.Show("Por favor selecciona un insumo válido y una cantidad mayor a 0.");
            }
        }
        private void ClearOrderIfNeeded()
        {
            if (orderItems.Count > 0)
            {
                orderItems.Clear();
                OrdersuppliersDataGrid.ItemsSource = null;
            }
        }
        private void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersuppliersDataGrid.SelectedItem is OrderSupplyItem selectedItem)
            {
                orderItems.Remove(selectedItem);
                OrdersuppliersDataGrid.ItemsSource = null;
                OrdersuppliersDataGrid.ItemsSource = orderItems;
            }
            else
            {
                //MessageBox.Show("Selecciona un insumo del listado para eliminar.", "Eliminar insumo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
