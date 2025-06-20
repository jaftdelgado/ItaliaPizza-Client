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

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class RegisterSupplyWaste : UserControl
    {
        private List<Supply> _allSupplies = new List<Supply>();
        private Supply _selectedSupply;
        public RegisterSupplyWaste()
        {
            InitializeComponent();
            Loaded += RegisterSupplyWaste_Loaded;
        }

        public static void Show(FrameworkElement triggerButton)
        {
            var RegisterWasteSupply = new RegisterSupplyWaste();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var popupContainer = mainWindow.PopUpHost.Parent as FrameworkElement;

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            RegisterWasteSupply.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = RegisterWasteSupply.DesiredSize.Width;
            double popupHeigth = RegisterWasteSupply.DesiredSize.Height;

            double left = mainWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            mainWindow.PopUpHost.Content = RegisterWasteSupply;
            Canvas.SetLeft(mainWindow.PopUpHost, left);
            Canvas.SetTop(mainWindow.PopUpHost, top);

            mainWindow.PopUpOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(RegisterWasteSupply, "ShowBorderAnimation");
        }
        private void ClosePopup()
        {
            var activeWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow == null) return;

            var popUpHost = activeWindow.FindName("PopUpHost") as ContentControl;
            var popUpOverlay = activeWindow.FindName("PopUpOverlay") as UIElement;

            if (popUpHost != null) popUpHost.Content = null;
            if (popUpOverlay != null) popUpOverlay.Visibility = Visibility.Collapsed;
        }

        private void RegisterSupplyWaste_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliesForComboBox();

            var textBox = (TextBox)CbSupply.Template.FindName("PART_EditableTextBox", CbSupply);
            if (textBox != null)
                textBox.TextChanged += CbSupply_TextChanged;
        }

        private async void LoadSuppliesForComboBox()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllSupplies(false);

                var supplies = dtoList.Select(s => new Supply
                {
                    Id = s.Id,
                    Name = s.Name,
                    Stock = s.Stock
                }).ToList();

                _allSupplies = supplies;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SetSuppliesComboBox();
                });
            });
        }

        private void SetSuppliesComboBox()
        {
            CbSupply.ItemsSource = _allSupplies;
            CbSupply.DisplayMemberPath = "Name";
        }
        private void CbSupply_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = CbSupply.Text.Trim().ToLower();

            var filtered = _allSupplies
            .Where(s => !string.IsNullOrWhiteSpace(s.Name) && s.Name.ToLower().Contains(searchText))
            .ToList();

            CbSupply.ItemsSource = filtered;
            CbSupply.IsDropDownOpen = true;
        }

        private void CbSupplyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbSupply.SelectedItem is Supply selected)
            {
                _selectedSupply = selected;
            }
        }

        private async Task RegisterSupplyLoss(Supply selected)
        {
            if (selected == null)
            {
                MessageDialog.Show("Waste_DialogTNoSelection", "Waste_DialogDNoSelection", AlertType.WARNING);
                return;
            }

            if (!decimal.TryParse(TbQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                MessageDialog.Show("Waste_DialogTNoQuantity", "Waste_DialogDNoQuantity", AlertType.WARNING);
                return;
            }

            if (quantity > selected.Stock)
            {
                MessageDialog.Show("Waste_DialogTExceed", "Waste_DialogDExceed", AlertType.WARNING);
                return;
            }

            var updatedStock = selected.Stock - quantity;

            var supplyDTO = new SupplyDTO
            {
                Id = selected.Id,
                Stock = updatedStock
            };

            var client = ServiceClientManager.Instance.Client;
            if (client == null)
            {
                MessageDialog.Show("GlbDialogT_NoConnection", "GlbDialogD_NoConnection", AlertType.ERROR);
                return;
            }

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                success = await Task.Run(() => client.RegisterSupplyLoss(supplyDTO));
                MessageDialog.Show("Waste_DialogTSuccess", "Waste_DialogDSuccess", AlertType.SUCCESS);
            });

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (success)
                {
                    MessageDialog.Show("Waste_DialogTSuccess", "Waste_DialogDSuccess", AlertType.SUCCESS);
                }
                else
                {
                    MessageDialog.Show("Waste_DialogTError", "Waste_DialogDError", AlertType.ERROR);
                }
            });
        }

        private void TbQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!decimal.TryParse(TbQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                TbQuantity.BorderBrush = Brushes.Red;
                TbQuantity.ToolTip = "Enter a valid positive number";
            }
            else
            {
                TbQuantity.ClearValue(Border.BorderBrushProperty);
                TbQuantity.ToolTip = null;
            }
        }

        private async void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            var selected = CbSupply.SelectedItem as Supply;
            await RegisterSupplyLoss(selected);
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }
    }
}
