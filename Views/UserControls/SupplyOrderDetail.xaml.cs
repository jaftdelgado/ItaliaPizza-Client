using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class SupplyOrderDetail : UserControl
    {
        private decimal _originalQuantity;

        public SupplyOrderDetail()
        {
            InitializeComponent();
            DataContext = this;
            AssignEventHandlers();

            QuantityChanged += (s, q) => OnPropertyChanged(nameof(QuantityText));
            UpdateReadOnlyState();
        }

        public static readonly DependencyProperty SupplyNameProperty =
            DependencyProperty.Register(nameof(SupplyName), typeof(string), typeof(SupplyOrderDetail), new PropertyMetadata(""));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(SupplyOrderDetail), new PropertyMetadata(""));

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register(nameof(Price), typeof(decimal), typeof(SupplyOrderDetail), new PropertyMetadata(0m));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(SupplyOrderDetail), new PropertyMetadata(null));

        public static readonly DependencyProperty QuantityProperty =
            DependencyProperty.Register(nameof(Quantity), typeof(decimal), typeof(SupplyOrderDetail),
                new FrameworkPropertyMetadata(1m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnQuantityChanged));

        public static readonly DependencyProperty SubtotalProperty =
            DependencyProperty.Register(nameof(Subtotal), typeof(decimal), typeof(SupplyOrderDetail), new PropertyMetadata(0m));

        public static readonly DependencyProperty MeasureUnitIdProperty =
            DependencyProperty.Register(nameof(MeasureUnitId), typeof(int), typeof(SupplyOrderDetail),
                new PropertyMetadata(0, OnMeasureUnitIdChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SupplyOrderDetail), new PropertyMetadata(false, OnIsReadOnlyChanged));

        public string SupplyName
        {
            get => (string)GetValue(SupplyNameProperty);
            set => SetValue(SupplyNameProperty, value);
        }

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public decimal Price
        {
            get => (decimal)GetValue(PriceProperty);
            set => SetValue(PriceProperty, value);
        }

        public decimal Quantity
        {
            get => (decimal)GetValue(QuantityProperty);
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

        public int MeasureUnitId
        {
            get => (int)GetValue(MeasureUnitIdProperty);
            set => SetValue(MeasureUnitIdProperty, value);
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
            get => $"x {Quantity:0.00} {Unit}";
            set
            {
                if (decimal.TryParse(value, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var parsed) && parsed != 0) Quantity = parsed;

                else Quantity = _originalQuantity;
            }
        }

        public event EventHandler<decimal> QuantityChanged;
        public event EventHandler DeleteClicked;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private static void OnQuantityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SupplyOrderDetail)d;
            if (e.NewValue is decimal newQty)
            {
                control.Subtotal = control.Price * newQty;
                control.QuantityChanged?.Invoke(control, newQty);
                control.OnPropertyChanged(nameof(QuantityText));
            }
        }

        private static void OnMeasureUnitIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SupplyOrderDetail)d;
            var id = (int)e.NewValue;
            control.Unit = MeasureUnit.GetDefaultMeasureUnits().FirstOrDefault(mu => mu.Id == id)?.Abbreviation ?? "u";
            control.OnPropertyChanged(nameof(QuantityText));
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SupplyOrderDetail)d).UpdateReadOnlyState();
        }

        private void UpdateReadOnlyState()
        {
            bool isReadOnly = IsReadOnly;
            TbQuantity.IsReadOnly = isReadOnly;
            TbQuantity.IsHitTestVisible = !isReadOnly;
            BtnDelete.Visibility = BtnEdit.Visibility = isReadOnly ? Visibility.Collapsed : Visibility.Visible;
            SupplyTitle.FontSize = isReadOnly ? 13 : 14;
        }

        private void QuantityTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsReadOnly)
            {
                TbQuantity.Text = Quantity.ToString("0.###", CultureInfo.InvariantCulture);
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
            if (!decimal.TryParse(TbQuantity.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var val) || val == 0)
            {
                Quantity = _originalQuantity;
            }
            else
            {
                if (val > 999) val = 999;
                if (IsIntegerOnlyUnit()) val = Math.Floor(val);
                Quantity = val;
            }

            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);
            TbQuantity.Text = QuantityText;
        }

        private bool IsValidQuantityInput(string input)
        {
            string pattern = IsIntegerOnlyUnit() ? @"^\d{0,4}$" : @"^\d{0,4}(\.\d{0,2})?$";
            return Regex.IsMatch(input, pattern) &&
                   (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) || val <= 9999) &&
                   (!IsIntegerOnlyUnit() || !input.Contains('.'));
        }

        private bool IsIntegerOnlyUnit() => MeasureUnitId == 3;

        private void EnableEditMode(bool enable)
        {
            TbQuantity.IsEnabled = enable;
            if (enable)
            {
                TbQuantity.Focus();
            }

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

        public void SetOriginalQuantity(decimal quantity) => _originalQuantity = quantity;

        public void RefreshBinding()
        {
            TbQuantity.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
