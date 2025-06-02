using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views.OrdersModule
{
    public partial class ProductOrderDetail : UserControl, INotifyPropertyChanged
    {
        private int _originalQuantity;

        public ProductOrderDetail()
        {
            InitializeComponent();
            DataContext = this;
            AssignEventHandlers();

            QuantityChanged += (s, q) => OnPropertyChanged(nameof(QuantityText));
            UpdateReadOnlyState();
        }

        public static readonly DependencyProperty ProductNameProperty =
            DependencyProperty.Register(nameof(ProductName), typeof(string), typeof(ProductOrderDetail), new PropertyMetadata(""));

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register(nameof(Price), typeof(decimal), typeof(ProductOrderDetail), new PropertyMetadata(0m));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(ProductOrderDetail), new PropertyMetadata(null));

        public static readonly DependencyProperty QuantityProperty =
            DependencyProperty.Register(nameof(Quantity), typeof(int), typeof(ProductOrderDetail),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnQuantityChanged));

        public static readonly DependencyProperty SubtotalProperty =
            DependencyProperty.Register(nameof(Subtotal), typeof(decimal), typeof(ProductOrderDetail), new PropertyMetadata(0m));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ProductOrderDetail), new PropertyMetadata(false, OnIsReadOnlyChanged));

        public string ProductName
        {
            get => (string)GetValue(ProductNameProperty);
            set => SetValue(ProductNameProperty, value);
        }

        public decimal Price
        {
            get => (decimal)GetValue(PriceProperty);
            set => SetValue(PriceProperty, value);
        }

        public int Quantity
        {
            get => (int)GetValue(QuantityProperty);
            set
            {
                SetValue(QuantityProperty, value);
                _originalQuantity = value;
            }
        }

        public decimal Subtotal
        {
            get => (decimal)GetValue(SubtotalProperty);
            set => SetValue(SubtotalProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public string QuantityText
        {
            get => $"x {Quantity}";
            set
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
                {
                    Quantity = parsed;
                }
                else
                {
                    Quantity = _originalQuantity;
                }
            }
        }

        public event EventHandler<int> QuantityChanged;
        public event EventHandler DeleteClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnQuantityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ProductOrderDetail)d;
            if (e.NewValue is int newQty)
            {
                control.Subtotal = control.Price * newQty;
                control.QuantityChanged?.Invoke(control, newQty);
                control.OnPropertyChanged(nameof(QuantityText));
            }
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductOrderDetail)d).UpdateReadOnlyState();
        }

        private void UpdateReadOnlyState()
        {
            bool isReadOnly = IsReadOnly;
            TbQuantity.IsReadOnly = isReadOnly;
            TbQuantity.IsHitTestVisible = !isReadOnly;
            BtnDelete.Visibility = BtnEdit.Visibility = isReadOnly ? Visibility.Collapsed : Visibility.Visible;
            ProductTitle.FontSize = isReadOnly ? 13 : 14;
        }

        private void QuantityTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsReadOnly)
            {
                TbQuantity.Text = Quantity.ToString(CultureInfo.InvariantCulture);
                TbQuantity.SelectAll();
            }
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsReadOnly)
            {
                ConfirmQuantityChange();
            }
        }

        private void Click_BtnDelete(object sender, RoutedEventArgs e) => DeleteClicked?.Invoke(this, EventArgs.Empty);

        private void Click_BtnEdit(object sender, RoutedEventArgs e)
        {
            EnableEditMode(true);
        }

        private void Click_BtnConfirm(object sender, RoutedEventArgs e)
        {
            ConfirmQuantityChange();
            EnableEditMode(false);
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsReadOnly)
            {
                string currentText = TbQuantity.Text.Remove(TbQuantity.SelectionStart, TbQuantity.SelectionLength)
                                        .Insert(TbQuantity.SelectionStart, e.Text);

                if (!IsValidQuantityInput(currentText))
                {
                    e.Handled = true;
                    Animations.ShakeTextBox(TbQuantity);
                }
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!IsReadOnly && e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var pasteText = e.DataObject.GetData(DataFormats.Text) as string;
                if (!IsValidQuantityInput(pasteText))
                {
                    e.CancelCommand();
                    Animations.ShakeTextBox(TbQuantity);
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void ConfirmQuantityChange()
        {
            if (!int.TryParse(TbQuantity.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) || val <= 0)
            {
                Quantity = _originalQuantity;
            }
            else
            {
                if (val > 999) val = 999;
                Quantity = val;
            }

            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);
            TbQuantity.Text = QuantityText;
        }

        private bool IsValidQuantityInput(string input)
        {
            return Regex.IsMatch(input, @"^\d{0,3}$") &&
                   (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) || val <= 999);
        }

        private void EnableEditMode(bool enable)
        {
            TbQuantity.IsEnabled = enable;

            if (enable) TbQuantity.Focus();

            BtnConfirm.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
            BtnDelete.Visibility = BtnEdit.Visibility = enable ? Visibility.Collapsed : Visibility.Visible;
        }

        public void AssignEventHandlers()
        {
            TbQuantity.GotFocus += QuantityTextBox_GotFocus;
            TbQuantity.LostFocus += QuantityTextBox_LostFocus;
            TbQuantity.PreviewTextInput += QuantityTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(TbQuantity, OnPaste);

            Loaded += (s, e) => Application.Current.MainWindow.PreviewMouseDown += HandleGlobalClick;
            Unloaded += (s, e) => Application.Current.MainWindow.PreviewMouseDown -= HandleGlobalClick;
        }

        private void HandleGlobalClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsReadOnly && TbQuantity.IsEnabled && e.OriginalSource is DependencyObject clickedElement && !IsDescendantOf(this, clickedElement))
            {
                ConfirmQuantityChange();
                EnableEditMode(false);
                Keyboard.ClearFocus();
            }
        }

        private bool IsDescendantOf(DependencyObject parent, DependencyObject child)
        {
            while (child != null)
            {
                if (child == parent) return true;
                child = VisualTreeHelper.GetParent(child);
            }
            return false;
        }

        public void SetOriginalQuantity(int quantity) => _originalQuantity = quantity;

        public void RefreshBinding()
        {
            TbQuantity.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}