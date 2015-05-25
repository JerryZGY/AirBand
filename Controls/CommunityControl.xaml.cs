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
using Facebook;

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

        private String accessToken = null;

        private void toggleButton_Click (object sender, RoutedEventArgs e)
        {
            var toggleButton = (System.Windows.Controls.Primitives.ToggleButton)sender;
            if ((Boolean)toggleButton.IsChecked)
            {
                StoryboardHandler.InitStoryBoard(this, "ToggleInStoryboard");
                WebBrowser.Navigate("https://graph.facebook.com/oauth/authorize?client_id=1575132309420044&redirect_uri=http://www.facebook.com/connect/login_success.html&type=user_agent&display=popup");
            }
            else
            {
                if (accessToken != null)
                {
                    var webBrowser = new WebBrowser();
                    webBrowser.Navigate(new FacebookClient().GetLogoutUrl(new
                    {
                        next = "https://www.facebook.com/connect/login_success.html",
                        access_token = accessToken
                    }));
                    accessToken = null;
                    Title = "Facebook";
                    Desc = "連接您的Facebook帳號來分享演奏資訊";
                    Url = "/KinectAirBand;component/Resources/Facebook.jpg";
                }
                else
                    StoryboardHandler.InitStoryBoard(this, "ToggleOutStoryboard");
            }
        }

        private void WebBrowser_Navigated (object sender, NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("https://www.facebook.com/dialog/oauth"))
            {
                WebBrowser.Visibility = System.Windows.Visibility.Visible;
            }
            if (e.Uri.ToString().StartsWith("http://www.facebook.com/connect/login_success.html"))
            {
                WebBrowser.Visibility = System.Windows.Visibility.Collapsed;
                accessToken = e.Uri.Fragment.Split('&')[0].Replace("#access_token=", "");
                FacebookClient FBClient = new FacebookClient(accessToken);
                dynamic me = FBClient.Get("me");
                Title = String.Format("{0} {1}", me.first_name, me.last_name);
                Desc = "已連接，您可以在演奏時分享圖片了！";
                me = FBClient.Get("me/picture?width=100&height=100&redirect=false");
                Url = me.data.url;
                StoryboardHandler.InitStoryBoard(this, "ToggleOutStoryboard");
            }
        }

        private void UserControl_LostFocus (object sender, RoutedEventArgs e)
        {
            if (accessToken == null && (Boolean)toggleButton.IsChecked)
            {
                StoryboardHandler.InitStoryBoard(this, "RestoreStoryboard");
                toggleButton.IsChecked = false;
            }
        }
    }
}
