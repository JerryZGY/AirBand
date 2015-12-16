using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Facebook;

namespace AirBand.Controls
{
    public partial class Ctrl_CommunityElement : UserControl
    {
        public Ctrl_CommunityElement()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(Ctrl_CommunityElement), new PropertyMetadata(""));
        public string Url
        {
            get { return GetValue(UrlProperty).ToString(); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Ctrl_CommunityElement), new PropertyMetadata(""));
        public string Title
        {
            get { return GetValue(TitleProperty).ToString(); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty DescProperty = DependencyProperty.Register("Desc", typeof(string), typeof(Ctrl_CommunityElement), new PropertyMetadata(""));
        public string Desc
        {
            get { return GetValue(DescProperty).ToString(); }
            set { SetValue(DescProperty, value); }
        }

        public static readonly DependencyProperty EnableProperty = DependencyProperty.Register("Enable", typeof(string), typeof(Ctrl_CommunityElement), new PropertyMetadata(""));
        public string Enable
        {
            get { return GetValue(EnableProperty).ToString(); }
            set { SetValue(EnableProperty, value); }
        }

        public bool TestEnable
        {
            get { return !Convert.ToBoolean(Enable); }
        }

        public bool TestDisable
        {
            get { return Convert.ToBoolean(Enable); }
        }

        private string accessToken = null;

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = (System.Windows.Controls.Primitives.ToggleButton)sender;
            if ((bool)toggleButton.IsChecked)
            {
                this.Begin("ToggleIn");
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
                    Url = "/AirBand;component/Resources/Facebook.jpg";
                }
                else
                    this.Begin("ToggleOut");
            }
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("https://www.facebook.com/dialog/oauth"))
            {
                WebBrowser.Visibility = Visibility.Visible;
            }
            if (e.Uri.ToString().StartsWith("http://www.facebook.com/connect/login_success.html"))
            {
                WebBrowser.Visibility = Visibility.Collapsed;
                accessToken = e.Uri.Fragment.Split('&')[0].Replace("#access_token=", "");
                FacebookClient FBClient = new FacebookClient(accessToken);
                dynamic me = FBClient.Get("me");
                Title = string.Format("{0} {1}", me.first_name, me.last_name);
                Desc = "已連接，您可以在演奏時分享圖片了！";
                me = FBClient.Get("me/picture?width=100&height=100&redirect=false");
                Url = me.data.url;
                this.Begin("ToggleOut");
            }
        }
    }
}