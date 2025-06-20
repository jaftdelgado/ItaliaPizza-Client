using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public bool IsSingleSelection { get; set; } = false;

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

            SelectionCompleted?.Invoke(this, SelectedSupplies);
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
                var card = CreateSupplyCard(supply);
                if (SelectedSupplies.Any(s => s.Id == supply.Id))
                {
                    card.IsSelected = true;
                    card.OnSelectionChanged(true);
                }

                SupplyCards.Add(card);
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
                if (IsSingleSelection && isChecked)
                {
                    foreach (var otherCard in SupplyCards.Where(c => c != card && c.IsSelected))
                    {
                        otherCard.IsSelected = false;
                        var deselectedSupply = _allSupplies.FirstOrDefault(s => s.Name == otherCard.SupplyName);
                        if (deselectedSupply != null)
                        {
                            SelectedSupplies.RemoveAll(s => s.Id == deselectedSupply.Id);
                            SupplySelectionChanged?.Invoke(deselectedSupply, false);
                        }
                    }

                    SelectedSupplies.Clear();
                    SelectedSupplies.Add(supply);
                    SupplySelectionChanged?.Invoke(supply, true);

                    ClosePopup();
                }
                else if (isChecked)
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

    public class SupplyCard : INotifyPropertyChanged
    {
        private string _supplyName;
        private string _category;
        private BitmapImage _imageSource;
        private bool _isSelected;

        public string SupplyName
        {
            get => _supplyName;
            set
            {
                _supplyName = value;
                OnPropertyChanged(nameof(SupplyName));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public BitmapImage ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public delegate void SelectionChangedHandler(bool isChecked);
        public event SelectionChangedHandler SelectionChanged;

        public void OnSelectionChanged(bool isChecked)
        {
            IsSelected = isChecked;
            SelectionChanged?.Invoke(isChecked);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}