using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace AirBand
{
    public static class JStoryteller
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
    }
}