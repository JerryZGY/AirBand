using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace KinectAirBand
{
    public static class StoryboardHandler
    {
        public static void InitStoryBoard (FrameworkElement element, String name, Action callback)
        {
            Storyboard storyBoard = ( (Storyboard)element.Resources[name] );
            storyBoard.Completed += (se, ev) => callback();
            storyBoard.Begin();
        }

        public static void InitHitStoryBoard (FrameworkElement element, String name)
        {
            element.IsHitTestVisible = false;
            Storyboard storyBoard = ( (Storyboard)element.Resources[name] );
            storyBoard.Completed += (se, ev) => element.IsHitTestVisible = true;
            storyBoard.Begin();
        }

        public static void InitHitStoryBoard (FrameworkElement element, String name, Action callback)
        {
            element.IsHitTestVisible = false;
            Storyboard storyBoard = ( (Storyboard)element.Resources[name] );
            storyBoard.Completed += (se, ev) =>
            {
                callback();
                element.IsHitTestVisible = true;
            };
            storyBoard.Begin();
        }
    }
}
