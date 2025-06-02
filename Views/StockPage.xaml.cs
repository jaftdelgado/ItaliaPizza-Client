using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
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
            LoadSuppliesData();
            InventoryTotalValue.Text = "0.00";
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
                    decimal totalValue = _allSupplies.Sum(s => s.Price * (s.Stock ?? 0));
                    InventoryTotalValue.Text = totalValue.ToString("C2");
                    UpdateElementsCounter(_allSupplies.Count);
                    ConfigureComboBoxes();
                });
            });
        }

        private void ConfigureComboBoxes()
        {
            CategoryComboBox.Items.Clear();
            SupplierComboBox.Items.Clear();

            CategoryComboBox.SelectionChanged += cb_SelectionCategoryChanged;
            SupplierComboBox.SelectionChanged += cb_SelectionSupplierChanged;

            CategoryComboBox.Items.Add(new ComboBoxItem { Content = "Todos" });
            SupplierComboBox.Items.Add(new ComboBoxItem { Content = "Todos" });

            var suppliers = _allSupplies
                .Select(s => s.SupplierName)
                .Distinct()
                .OrderBy(name => name);

            foreach (var supplier in suppliers)
            {
                SupplierComboBox.Items.Add(new ComboBoxItem { Content = supplier });
            }

            var categoryMap = SupplyCategory.GetDefaultSupplyCategories()
                .ToDictionary(c => c.Id.ToString(), c => c.Name);

            var categoryIds = _allSupplies
                .Select(s => s.SupplyCategoryID.ToString())
                .Distinct()
                .OrderBy(id => id);

            foreach (var categoryId in categoryIds)
            {
                var categoryName = categoryMap.TryGetValue(categoryId, out var name) ? name : "Unknown";

                CategoryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = categoryName,
                    Tag = categoryId
                });
            }

            CategoryComboBox.SelectedIndex = 0;
            SupplierComboBox.SelectedIndex = 0;
        }

        private void UpdateElementsCounter(int count)
        {
            ElementsCounter.Content = count.ToString();
        }

        [Obsolete]
        private void Click_BtnExportReport(object sender, RoutedEventArgs e)
        {
            if (_allSupplies == null || !_allSupplies.Any())
            {
                MessageDialog.Show("No hay datos para exportar.", "Información", AlertType.WARNING);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = "Reporte_Inventario_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ReportUtils.ExportarReporteInventario(_allSupplies, saveFileDialog.FileName);
                    MessageDialog.Show("Reporte exportado exitosamente.", "El reporte de inventarios se exportó con exito", AlertType.SUCCESS);
                }
                catch (Exception ex)
                {
                    MessageDialog.Show("Error al exportar el reporte: " + ex.Message, "Error", AlertType.ERROR);
                }
            }
        }

        private void cb_SelectionSupplierChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SupplierComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedSupplier = selectedItem.Content.ToString();
                if (selectedSupplier == "Todos")
                {
                    SupplyDataGrid.ItemsSource = _allSupplies;
                }
                else
                {
                    var filteredSupplies = _allSupplies.Where(s => s.SupplierName == selectedSupplier).ToList();
                    SupplyDataGrid.ItemsSource = filteredSupplies;
                }
            }
            else
            {
                SupplyDataGrid.ItemsSource = _allSupplies;
            }
            UpdateElementsCounter(SupplyDataGrid.Items.Count);
        }

        private void cb_SelectionCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content.ToString() == "Todos")
                {
                    SupplyDataGrid.ItemsSource = _allSupplies;
                }
                else
                {
                    var selectedCategoryId = selectedItem.Tag as string;
                    var filteredSupplies = _allSupplies
                        .Where(s => s.SupplyCategoryID.ToString() == selectedCategoryId)
                        .ToList();
                    SupplyDataGrid.ItemsSource = filteredSupplies;
                }
            }
            else
            {
                SupplyDataGrid.ItemsSource = _allSupplies;
            }
            UpdateElementsCounter(SupplyDataGrid.Items.Count);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                SupplyDataGrid.ItemsSource = _allSupplies;
            }
            else
            {
                var filteredSupplies = _allSupplies
                    .Where(s =>
                        (s.Name?.ToLower() ?? "").Contains(searchText) ||
                        (s.Brand?.ToLower() ?? "").Contains(searchText) ||
                        (s.Description?.ToLower() ?? "").Contains(searchText))
                    .ToList();
                SupplyDataGrid.ItemsSource = filteredSupplies;
            }
            UpdateElementsCounter(SupplyDataGrid.Items.Count);
        }
    }
}
