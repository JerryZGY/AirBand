using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectAirBand.Controls
{
    /// <summary>
    /// CommunityControl.xaml 的互動邏輯
    /// </summary>
    public partial class CommunityControl : UserControl
    {
        public CommunityControl ()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event EventHandler<RoutedEventArgs> PianoKeyDown;

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(CommunityControl), new PropertyMetadata(""));
        public String Url
        {
            get { return (String)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CommunityControl), new PropertyMetadata(""));
        public String Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty DescProperty = DependencyProperty.Register("Desc", typeof(string), typeof(CommunityControl), new PropertyMetadata(""));
        public String Desc
        {
            get { return (String)GetValue(DescProperty); }
            set { SetValue(DescProperty, value); }
        }

        public static readonly DependencyProperty EnableProperty = DependencyProperty.Register("Enable", typeof(string), typeof(CommunityControl), new PropertyMetadata(""));
        public String Enable
        {
            get { return (String)GetValue(EnableProperty); }
            set { SetValue(EnableProperty, value); }
        }

        private void toggleButton_Click (object sender, RoutedEventArgs e)
        {
            var toggleButton = (System.Windows.Controls.Primitives.ToggleButton)sender;
            if ((Boolean)toggleButton.IsChecked)
            {
                StoryboardHandler.InitStoryBoard(this, "ToggleInStoryboard");
                WebBrowser.Navigate("https://graph.facebook.com/oauth/authorize?client_id=1575132309420044&redirect_uri=http://www.facebook.com/connect/login_success.html&type=user_agent&display=popup");
            }
            else
                StoryboardHandler.InitStoryBoard(this, "ToggleOutStoryboard");
        }
    }
}
