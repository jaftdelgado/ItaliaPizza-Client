using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class SupplyCard : UserControl
    {
        public SupplyCard()
        {
            InitializeComponent();
        }

        public string SupplyName
        {
            get => (string)GetValue(SupplyNameProperty);
            set => SetValue(SupplyNameProperty, value);
        }

        public static readonly DependencyProperty SupplyNameProperty =
            DependencyProperty.Register("SupplyName", typeof(string), typeof(SupplyCard), new PropertyMetadata(""));

        public string StockText
        {
            get => (string)GetValue(StockTextProperty);
            set => SetValue(StockTextProperty, value);
        }

        public static readonly DependencyProperty StockTextProperty =
            DependencyProperty.Register("StockText", typeof(string), typeof(SupplyCard), new PropertyMetadata(""));

        public string PriceText
        {
            get => (string)GetValue(PriceTextProperty);
            set => SetValue(PriceTextProperty, value);
        }

        public static readonly DependencyProperty PriceTextProperty =
            DependencyProperty.Register("PriceText", typeof(string), typeof(SupplyCard), new PropertyMetadata(""));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(SupplyCard), new PropertyMetadata(null));
    }
}
