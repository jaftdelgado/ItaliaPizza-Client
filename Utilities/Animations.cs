using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ItaliaPizzaClient.Utilities
{
    public static class Animations
    {
        public static void BeginAnimation(UIElement targetElement, string resourceKey, Action onComplete = null)
        {
            if (targetElement == null || string.IsNullOrEmpty(resourceKey)) return;

            var animation = Application.Current.Resources[resourceKey] as Storyboard;
            if (animation != null)
            {
                var clonedAnimation = animation.Clone();

                if (onComplete != null)
                    clonedAnimation.Completed += (s, e) => onComplete();

                if (targetElement is FrameworkElement frameworkElement)
                    clonedAnimation.Begin(frameworkElement);
            }
        }
    }
}
