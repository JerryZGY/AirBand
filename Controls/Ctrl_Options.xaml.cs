using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AirBand.Controls
{
    public partial class Ctrl_Options : UserControl
    {
        public Ctrl_Options()
        {
            InitializeComponent();
            DataContext = Switcher.VM_EnvironmentVariables;
        }

        private void ctrl_Loaded(object sender, RoutedEventArgs e)
        {
            EnterStory();
        }

        private void EnterStory()
        {
            var story = new Storyboard();
            for (int i = 0; i < Stp_Left.Children.Count; i++)
            {
                var leftElement = Stp_Left.Children[i];
                leftElement.RenderTransform = Stp_Right.Children[i].RenderTransform = new TranslateTransform(100, 0);
                var doubleAnimation = new DoubleAnimation()
                {
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(0.05 * i),
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };
                story.Children.Add(doubleAnimation);
                Storyboard.SetTarget(doubleAnimation, leftElement);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
            }
            story.Begin();
        }
    }
}