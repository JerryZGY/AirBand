using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AirBand.Controls
{
    public partial class Ctrl_Community : UserControl
    {
        public Ctrl_Community()
        {
            InitializeComponent();
        }

        private void ctrl_Loaded(object sender, RoutedEventArgs e)
        {
            EnterStory();
        }

        private void EnterStory()
        {
            var story = new Storyboard();
            for (int i = 0; i < Grid_Elements.Children.Count; i++)
            {
                var element = Grid_Elements.Children[i];
                element.RenderTransform = new TranslateTransform(100, 0);
                var doubleAnimation = new DoubleAnimation()
                {
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(0.05 * i),
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };
                story.Children.Add(doubleAnimation);
                Storyboard.SetTarget(doubleAnimation, element);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
            }
            story.Begin();
        }
    }
}