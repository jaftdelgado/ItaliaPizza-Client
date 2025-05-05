using System.Windows.Controls;
using ItaliaPizzaClient.Views.Dialogs;
using System.Windows;
using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using System.Collections.Generic;
using System;
using System.Linq;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterOrderSupplierPage : Page
    {
        private readonly List<OrderItem> orderItems = new List<OrderItem>();
        private readonly MainManagerClient client = new MainManagerClient();

        public RegisterOrderSupplierPage()
        {
            InitializeComponent();
            btnAddSupply.IsEnabled = false;
            txtQuantity.TextChanged += (s, e) => CheckAddSupplyButtonState();
            LoadCategories();
            InputUtilities.ValidateDecimalInput(txtQuantity);

            cbSuppliersName.IsEnabled = false;
            cbSuppliesName.IsEnabled = false;
            txtQuantity.IsEnabled = false;
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
            ClearOrderIfNeeded();
            cbSuppliersName.IsEnabled = false;
            cbSuppliesName.Items.Clear();
            cbSuppliesName.SelectedIndex = -1;

            cbSuppliesName.IsEnabled = false;
            cbSuppliesName.Items.Clear();
            cbSuppliesName.SelectedIndex = -1;

            txtQuantity.IsEnabled = false;

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

                cbSuppliersName.IsEnabled = true;
            }
        }

        private void cbSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOrderIfNeeded();
            cbSuppliesName.IsEnabled = false;
            cbSuppliesName.Items.Clear();
            cbSuppliesName.SelectedIndex = -1;
            txtQuantity.IsEnabled = false;

            if (cbSuppliersName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplierDTO selectedSupplier)
            {
                int supplierId = selectedSupplier.Id;

                var supplies = client.GetSuppliesBySupplier(supplierId);

                cbSuppliesName.Items.Clear();

                foreach (var supply in supplies)
                {
                    bool alreadyAdded = orderItems.Any(o => o.SupplyName == supply.Name);
                    if (!alreadyAdded)
                    {
                        cbSuppliesName.Items.Add(new ComboBoxItem
                        {
                            Content = supply.Name,
                            Tag = supply
                        });
                    }
                }
            }
            cbSuppliesName.IsEnabled = true;
            CheckAddSupplyButtonState();
        }

        private void cbSuppliesName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSuppliesName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplyDTO selectedSupply)
            {
                txtQuantity.IsEnabled = true;
                txtQuantity.Focus();
            }
        }

        private void AddSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbSuppliesName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplyDTO selectedSupply &&
                decimal.TryParse(txtQuantity.Text, out decimal quantity) &&
                quantity > 0)
            {
                var allUnits = MeasureUnit.GetDefaultMeasureUnits();
                var measureUnit = allUnits.FirstOrDefault(mu => mu.Id == selectedSupply.MeasureUnit);

                orderItems.Add(new OrderItem
                {
                    SupplyName = selectedSupply.Name,
                    Quantity = quantity,
                    MeasureUnit = measureUnit?.Abbreviation
                });

                OrdersuppliersDataGrid.ItemsSource = null;
                OrdersuppliersDataGrid.ItemsSource = orderItems;

                txtQuantity.Clear();
                cbSuppliesName.SelectedIndex = -1;

                int supplierId = selectedSupply.Id;
                var supplies = client.GetSuppliesBySupplier(supplierId);

                cbSuppliesName.Items.Clear();
                foreach (var supply in supplies)
                {
                    if (!orderItems.Any(o => o.SupplyName == supply.Name))
                    {
                        cbSuppliesName.Items.Add(new ComboBoxItem
                        {
                            Content = supply.Name,
                            Tag = supply
                        });
                    }
                }
                txtQuantity.IsEnabled = false;
            }
            btnAddSupply.IsEnabled = false;
        }


        private void DeleteSelectedSupply_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersuppliersDataGrid.SelectedItem is OrderItem selectedItem)
            {
                orderItems.Remove(selectedItem);
                OrdersuppliersDataGrid.ItemsSource = null;
                OrdersuppliersDataGrid.ItemsSource = orderItems;

                RefreshSuppliesComboBox();
            }
            else
            {
                MessageDialog.Show("Eliminar Insumo", "Selecciona un insumo de la lista para eliminar.", AlertType.WARNING);
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
                dto.Items = items.ToArray();

                int result = client.RegisterOrder(dto);

                if (result == 1)
                {
                    MessageDialog.Show("Éxito", "Orden registrada exitosamente.", AlertType.SUCCESS);
                    orderItems.Clear();
                    OrdersuppliersDataGrid.ItemsSource = null;
                    txtQuantity.Clear();
                    cbSuppliesName.SelectedIndex = -1;
                }
                else
                {
                    MessageDialog.Show("Error", "Ocurrió un error al registrar la orden.", AlertType.ERROR);
                }
            }
            else
            {
                MessageDialog.Show("Validación", "Debes seleccionar un proveedor y añadir al menos un insumo.", AlertType.WARNING);
            }
        }
        private void RefreshSuppliesComboBox()
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;

            if (cbSuppliersName.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is SupplierDTO selectedSupplier)
            {
                var supplies = client.GetSuppliesBySupplier(selectedSupplier.Id);

                cbSuppliesName.Items.Clear();

                foreach (var supply in supplies)
                {
                    bool alreadyAdded = orderItems.Any(o => o.SupplyName == supply.Name);
                    if (!alreadyAdded)
                    {
                        cbSuppliesName.Items.Add(new ComboBoxItem
                        {
                            Content = supply.Name,
                            Tag = supply
                        });
                    }
                }
                cbSuppliesName.SelectedIndex = -1;
                txtQuantity.IsEnabled = false;
            }
        }
        private void CheckAddSupplyButtonState()
        {
            bool isSupplySelected = cbSuppliesName.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is SupplyDTO;
            bool isQuantityValid = decimal.TryParse(txtQuantity.Text, out decimal quantity) && quantity > 0;

            btnAddSupply.IsEnabled = isSupplySelected && isQuantityValid;
        }
    }
}