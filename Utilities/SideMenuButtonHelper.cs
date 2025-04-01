using System.Windows;

namespace ItaliaPizzaClient.Utilities
{
    public class SideMenuButtonHelper
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(SideMenuButtonHelper), new PropertyMetadata(false));

        public static void SetIsSelected(UIElement element, bool value)
        {
            element.SetValue(IsSelectedProperty, value);
        }

        // Obtiene el valor de la propiedad adjunta IsSelected
        public static bool GetIsSelected(UIElement element)
        {
            return (bool)element.GetValue(IsSelectedProperty);
        }
    }
}
