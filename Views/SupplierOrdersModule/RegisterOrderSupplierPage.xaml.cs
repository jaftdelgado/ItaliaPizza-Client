using System.Windows.Controls;
using System.Windows;
using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using System.Collections.Generic;
using System;
using System.Linq;
using ItaliaPizzaClient.ItaliasPizzaServices;


namespace ItaliaPizzaClient.Views
{
    public partial class RegisterOrderSupplierPage : Page
    {
        private readonly MainManagerClient client = new MainManagerClient();
        private readonly List<OrderItem> orderItems = new List<OrderItem>();


        public RegisterOrderSupplierPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = client.GetAllCategories();

            cbSuppliersCategories.Items.Clear();
            foreach (var category in categories)
            {
                cbSuppliersCategories.Items.Add(new ComboBoxItem
                {
                    Content = category.Name,
                    Tag = category
                });
            }
        }


        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOrderIfNeeded();
            if (cbSuppliersCategories.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplyCategoryDTO selectedCategory)
            {
                int categoryId = selectedCategory.Id;

                var suppliers = client.GetSuppliersByCategory(categoryId);
                cbSuppliersName.Items.Clear();

                foreach (var supplier in suppliers)
                {
                    cbSuppliersName.Items.Add(new ComboBoxItem
                    {
                        Content = supplier.ContactName,
                        Tag = supplier
                    });
                }

                cbSuppliesName.Items.Clear();
            }
        }

        private void cbSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOrderIfNeeded();
            if (cbSuppliersName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplierDTO selectedSupplier)
            {
                int supplierId = selectedSupplier.Id;

                var supplies = client.GetSuppliesBySupplier(supplierId);
                cbSuppliesName.Items.Clear();

                foreach (var supply in supplies)
                {
                    cbSuppliesName.Items.Add(new ComboBoxItem
                    {
                        Content = supply.Name,
                        Tag = supply
                    });
                }
            }
        }

        private void AddSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbSuppliesName.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is SupplyDTO selectedSupply && decimal.TryParse(txtQuantity.Text, out decimal quantity) && quantity > 0)
            {

                // Agrega el nuevo insumo
                orderItems.Add(new OrderItem
                {
                    SupplyName = selectedSupply.Name,
                    Quantity = quantity,
                    MeasureUnit = selectedSupply.MeasureUnit
                });

                OrdersuppliersDataGrid.ItemsSource = null;
                OrdersuppliersDataGrid.ItemsSource = orderItems;

                txtQuantity.Clear();
                cbSuppliesName.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("Selecciona un insumo y escribe una cantidad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteSelectedSupply_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersuppliersDataGrid.SelectedItem is OrderItem selectedItem)
            {
                orderItems.Remove(selectedItem);
                OrdersuppliersDataGrid.ItemsSource = null;
                OrdersuppliersDataGrid.ItemsSource = orderItems;
            }
            else
            {
                MessageBox.Show("Selecciona un insumo de la lista para eliminar.", "Eliminar Insumo", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void SubmitOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cbSuppliersName.SelectedItem is ComboBoxItem supplierItem &&
                supplierItem.Tag is SupplierDTO selectedSupplier &&
                orderItems.Count > 0)
            {
                var supplies = client.GetSuppliesBySupplier(selectedSupplier.Id);

                var dto = new SupplierOrderDTO
                {
                    SupplierID = selectedSupplier.Id,
                    OrderedDate = DateTime.Now,
                    Status = "En espera"
                };

                decimal total = 0;

                // ✅ Aquí va la lista temporal
                var items = new List<SupplierOrderDTO.OrderItemDTO>();

                foreach (var item in orderItems)
                {
                    var matchingSupply = supplies.FirstOrDefault(s => s.Name == item.SupplyName);
                    if (matchingSupply != null)
                    {
                        decimal lineTotal = item.Quantity * matchingSupply.Price;
                        total += lineTotal;

                        items.Add(new SupplierOrderDTO.OrderItemDTO
                        {
                            SupplyID = matchingSupply.Id,
                            Quantity = item.Quantity,
                            UnitPrice = matchingSupply.Price
                        });
                    }
                }

                dto.Total = total;
                dto.Items = items.ToArray(); // ✅ Se asigna como arreglo

                int result = client.RegisterOrder(dto);

                if (result == 1)
                {
                    MessageBox.Show("Orden registrada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    orderItems.Clear();
                    OrdersuppliersDataGrid.ItemsSource = null;
                    txtQuantity.Clear();
                    cbSuppliesName.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al registrar la orden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Debes seleccionar un proveedor y añadir al menos un insumo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
