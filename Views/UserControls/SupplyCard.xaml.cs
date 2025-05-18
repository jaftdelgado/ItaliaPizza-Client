using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class SupplyCard : UserControl
    {
        public event RoutedEventHandler CardClicked;

        public SupplyCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SupplyNameProperty =
            DependencyProperty.Register(nameof(SupplyName), typeof(string), typeof(SupplyCard), new PropertyMetadata(""));

        public static readonly DependencyProperty StockTextProperty =
            DependencyProperty.Register(nameof(StockText), typeof(string), typeof(SupplyCard), new PropertyMetadata(""));

        public static readonly DependencyProperty PriceTextProperty =
            DependencyProperty.Register(nameof(PriceText), typeof(string), typeof(SupplyCard), new PropertyMetadata(""));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(SupplyCard), new PropertyMetadata(null));

        public string SupplyName
        {
            get => (string)GetValue(SupplyNameProperty);
            set => SetValue(SupplyNameProperty, value);
        }

        public string StockText
        {
            get => (string)GetValue(StockTextProperty);
            set => SetValue(StockTextProperty, value);
        }

        public string PriceText
        {
            get => (string)GetValue(PriceTextProperty);
            set => SetValue(PriceTextProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CardClicked?.Invoke(this, new RoutedEventArgs());
        }
    }
}
