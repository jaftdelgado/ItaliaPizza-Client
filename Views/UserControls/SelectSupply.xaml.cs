using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class SelectSupply : UserControl
    {
        public ObservableCollection<SupplyCard> SupplyCards { get; } = new ObservableCollection<SupplyCard>();
        public List<Supply> SelectedSupplies { get; } = new List<Supply>();

        private List<Supply> _allSupplies = new List<Supply>();

        public event Action<Supply, bool> SupplySelectionChanged;

        public SelectSupply()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Show(FrameworkElement triggerButton)
        {
            var activeWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow == null) return;

            var popUpHost = activeWindow.FindName("PopUpHost") as ContentControl;
            var popUpOverlay = activeWindow.FindName("PopUpOverlay") as UIElement;
            var popupContainer = popUpHost?.Parent as FrameworkElement;

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = DesiredSize.Width;
            double popupHeight = DesiredSize.Height;

            double left = containerPos.X;
            double top = containerPos.Y + triggerButton.ActualHeight + 5;

            if (left + popupWidth > activeWindow.ActualWidth) left = activeWindow.ActualWidth - popupWidth - 10;

            if (top + popupHeight > activeWindow.ActualHeight) top = activeWindow.ActualHeight - popupHeight - 10;

            popUpHost.Content = this;
            Canvas.SetLeft(popUpHost, left);
            Canvas.SetTop(popUpHost, top);

            popUpOverlay.Visibility = Visibility.Visible;
            Animations.BeginAnimation(this, "ShowBorderAnimation");
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

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }

        public void SetSupplies(IEnumerable<Supply> allSupplies, IEnumerable<Supply> selectedSupplies)
        {
            if (allSupplies == null) return;

            _allSupplies = allSupplies.ToList();

            SelectedSupplies.Clear();
            if (selectedSupplies != null)
                SelectedSupplies.AddRange(selectedSupplies);

            SupplyCards.Clear();

            foreach (var supply in _allSupplies)
            {
                var isSelected = SelectedSupplies.Any(s => s.Id == supply.Id);
                SupplyCards.Add(CreateSupplyCard(supply, isSelected));
            }
        }

        private SupplyCard CreateSupplyCard(Supply supply, bool isSelected = false)
        {
            var card = new SupplyCard
            {
                SupplyName = supply.Name,
                Category = supply.CategoryName,
                ImageSource = ImageUtilities.ConvertToImageSource(supply.SupplyPic),
                IsSelected = isSelected
            };

            card.SelectionChanged += (isChecked) =>
            {
                if (isChecked)
                {
                    if (!SelectedSupplies.Any(s => s.Id == supply.Id))
                    {
                        SelectedSupplies.Add(supply);
                        SupplySelectionChanged?.Invoke(supply, true); 
                    }
                }
                else
                {
                    SelectedSupplies.RemoveAll(s => s.Id == supply.Id);
                    SupplySelectionChanged?.Invoke(supply, false); 
                }
            };

            return card;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TbSearch.Text.ToLower();
            var filtered = _allSupplies.Where(s =>
                s.Name.ToLower().Contains(searchText) ||
                s.CategoryName.ToLower().Contains(searchText));

            SupplyCards.Clear();
            foreach (var supply in filtered)
            {
                var isSelected = SelectedSupplies.Any(s => s.Id == supply.Id);
                SupplyCards.Add(CreateSupplyCard(supply, isSelected));
            }
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            ClosePopup();
            SelectionCompleted?.Invoke(this, SelectedSupplies);
        }

        public event EventHandler<List<Supply>> SelectionCompleted;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is SupplyCard card)
            {
                card.OnSelectionChanged(true);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is SupplyCard card)
            {
                card.OnSelectionChanged(false);
            }
        }
    }

    public class SupplyCard
    {
        public string SupplyName { get; set; }
        public string Category { get; set; }
        public BitmapImage ImageSource { get; set; }
        public bool IsSelected { get; set; }

        public delegate void SelectionChangedHandler(bool isChecked);
        public event SelectionChangedHandler SelectionChanged;

        public void OnSelectionChanged(bool isChecked)
        {
            SelectionChanged?.Invoke(isChecked);
        }
    }
}