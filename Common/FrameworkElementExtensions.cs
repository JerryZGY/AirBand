using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AirBand
{
    public static class FrameworkElementExtensions
    {
        public static void Begin(this FrameworkElement element, string name)
        {
            Storyboard story = ((Storyboard)element.Resources[name]);
            story.Begin();
        }

        public static void Begin(this FrameworkElement element, string name, Action completeAction)
        {
            Storyboard story = ((Storyboard)element.Resources[name]);
            story.Completed += (s, e) => completeAction();
            story.Begin();
        }

        public static Rect GetBounds(this FrameworkElement of, FrameworkElement from)
        {
            GeneralTransform transform = of.TransformToVisual(from);
            return transform.TransformBounds(new Rect(0, 0, of.ActualWidth, of.ActualHeight));
        }
    }
}