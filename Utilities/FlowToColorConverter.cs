using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ItaliaPizzaClient.Utilities
{
    public class FlowToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string flow)
            {
                var resources = Application.Current.Resources;
                if (flow == "I") return resources["SuccessBrush"] as Brush;
                
                else return resources["DangerBrush"] as Brush;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
