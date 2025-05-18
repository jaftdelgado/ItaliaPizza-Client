using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
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

            QuantityChanged += (s, q) =>
            {
                OnPropertyChanged(nameof(QuantityText));
            };

            UpdateReadOnlyState();
        }

        public static readonly DependencyProperty SupplyNameProperty =
            DependencyProperty.Register("SupplyName", typeof(string), typeof(SupplyOrderDetail), new PropertyMetadata(""));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(SupplyOrderDetail), new PropertyMetadata(""));

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(decimal), typeof(SupplyOrderDetail), new PropertyMetadata(0m));

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

        public event EventHandler<decimal> QuantityChanged;

        public event EventHandler DeleteClicked;

        private static void OnQuantityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SupplyOrderDetail)d;

            if (e.NewValue is decimal newQty)
            {
                control.Subtotal = control.Price * newQty;
                control.QuantityChanged?.Invoke(control, newQty);
                control.OnPropertyChanged(nameof(control.QuantityText));
            }
        }

        private static void OnMeasureUnitIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SupplyOrderDetail)d;

            int id = (int)e.NewValue;

            var unit = MeasureUnit.GetDefaultMeasureUnits().FirstOrDefault(mu => mu.Id == id);
            control.Unit = unit?.Abbreviation ?? "u";

            control.OnPropertyChanged(nameof(QuantityText));
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SupplyOrderDetail)d;
            control.UpdateReadOnlyState();
        }

        private void UpdateReadOnlyState()
        {
            bool isReadOnly = IsReadOnly;

            TbQuantity.IsReadOnly = isReadOnly;
            TbQuantity.IsHitTestVisible = !isReadOnly;
            BtnDelete.Visibility = isReadOnly ? Visibility.Collapsed : Visibility.Visible;
            BtnEdit.Visibility = isReadOnly ? Visibility.Collapsed : Visibility.Visible;
            SupplyTitle.FontSize = isReadOnly ? 13 : 14;
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

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

        public int MeasureUnitId
        {
            get => (int)GetValue(MeasureUnitIdProperty);
            set => SetValue(MeasureUnitIdProperty, value);
        }

        public string QuantityText
        {
            get => $"x {Quantity.ToString("0.00", CultureInfo.InvariantCulture)} {Unit}";
            set
            {
                if (string.IsNullOrWhiteSpace(value) || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal val) && val == 0)
                {
                    Quantity = _originalQuantity;
                }
                else if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                {
                    Quantity = parsed;
                }
            }
        }

        public decimal Subtotal
        {
            get => (decimal)GetValue(SubtotalProperty);
            set => SetValue(SubtotalProperty, value);
        }

        public void RefreshBinding()
        {
            var binding = TbQuantity.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateTarget();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void SetOriginalQuantity(decimal quantity)
        {
            _originalQuantity = quantity;
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
                string rawText = TbQuantity.Text.Trim();

                if (string.IsNullOrEmpty(rawText) || !decimal.TryParse(rawText, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) || val == 0)
                {
                    Quantity = _originalQuantity;
                }
                else
                {
                    if (val > 999) val = 999;
                    if (IsIntegerOnlyUnit()) val = Math.Floor(val);
                    Quantity = val;
                }

                TbQuantity.Text = $"x {Quantity.ToString("0.00", CultureInfo.InvariantCulture)} {Unit}";
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsReadOnly)
            {
                string pattern = IsIntegerOnlyUnit()
                    ? @"^\d{0,4}$"
                    : @"^\d{0,4}(\.\d{0,2})?$";

                var currentText = TbQuantity.Text.Remove(TbQuantity.SelectionStart, TbQuantity.SelectionLength)
                                    .Insert(TbQuantity.SelectionStart, e.Text);

                if (!Regex.IsMatch(currentText, pattern) ||
                    (decimal.TryParse(currentText, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) && val > 9999))
                {
                    e.Handled = true;
                    Animations.ShakeTextBox(TbQuantity);
                }

                if (IsIntegerOnlyUnit() && e.Text.Contains("."))
                {
                    e.Handled = true;
                    Animations.ShakeTextBox(TbQuantity);
                }
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!IsReadOnly)
            {
                if (e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    string pasteText = e.DataObject.GetData(DataFormats.Text) as string;

                    if (IsIntegerOnlyUnit() && pasteText.Contains('.'))
                    {
                        e.CancelCommand();
                        Animations.ShakeTextBox(TbQuantity);
                    }
                    else if (!Regex.IsMatch(pasteText, IsIntegerOnlyUnit() ? @"^\d{0,4}$" : @"^\d{0,4}(\.\d{0,2})?$"))
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
        }

        private bool IsIntegerOnlyUnit()
        {
            return MeasureUnitId == 3;
        }

        public void AssignEventHandlers()
        {
            TbQuantity.GotFocus += QuantityTextBox_GotFocus;
            TbQuantity.LostFocus += QuantityTextBox_LostFocus;
            TbQuantity.PreviewTextInput += QuantityTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(TbQuantity, OnPaste);
        }

        private void Click_BtnDelete(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
