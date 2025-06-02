using ItaliaPizzaClient.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ItaliaPizzaClient.Views.OrdersModule
{
    public partial class ProductCard : UserControl
    {
        public event RoutedEventHandler CardClicked;

        public ProductCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProductNameProperty =
            DependencyProperty.Register(nameof(ProductName), typeof(string), typeof(ProductCard), new PropertyMetadata(""));

        public static readonly DependencyProperty ProductCodeProperty =
            DependencyProperty.Register(nameof(ProductCode), typeof(string), typeof(ProductCard), new PropertyMetadata(""));

        public static readonly DependencyProperty PriceTextProperty =
            DependencyProperty.Register(nameof(PriceText), typeof(string), typeof(ProductCard), new PropertyMetadata(""));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(ProductCard), new PropertyMetadata(null));

        public string ProductName
        {
            get => (string)GetValue(ProductNameProperty);
            set => SetValue(ProductNameProperty, value);
        }

        public string ProductCode
        {
            get => (string)GetValue(ProductCodeProperty);
            set => SetValue(ProductCodeProperty, value);
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
            Animations.BeginAnimation(CardBorder, "ClickAnimation", () =>
            {
                CardClicked?.Invoke(this, new RoutedEventArgs());
            });
        }
    }
}