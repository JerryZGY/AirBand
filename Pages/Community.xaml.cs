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

namespace KinectAirBand.Pages
{
    /// <summary>
    /// Community.xaml 的互動邏輯
    /// </summary>
    public partial class Community : UserControl
    {
        public ImageSource ImageSource
        {
            get
            {
                return profilePicture;
            }
        }
        private FacebookClient FBClient;
        private const String clientId = "1575132309420044";
        private const String redirectUri = "http://www.facebook.com/connect/login_success.html";
        private const String responseType = "token";
        private const String display = "popup";
        private String accessToken { get; set; }
        private BitmapImage profilePicture;

        public Community (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            /*WebBrowser.Navigate(new Uri("https://graph.facebook.com/oauth/authorize?client_id=1575132309420044&redirect_uri=http://www.facebook.com/connect/login_success.html&type=user_agent&display=popup").AbsoluteUri);*/
        }

        private void Community_Loaded (object sender, RoutedEventArgs e)
        {
            /*FBClient = new FacebookClient();
            profilePicture = new BitmapImage();*/
        }

        private Uri getLoginUri ()
        {
            return FBClient.GetLoginUrl(new
            {
                client_id = clientId,
                redirect_uri = redirectUri,
                response_type = responseType,
                display = display
            });
        }

        private Uri getLogoutUri ()
        {
            return FBClient.GetLogoutUrl(new
            {
                next = redirectUri,
                access_token = accessToken
            });
        }

        private void WebBrowser_Navigated (object sender, NavigationEventArgs e)
        {
            /*if (e.Uri.ToString().StartsWith("http://www.facebook.com/connect/login_success.html"))
            {
                //WebBrowser.Visibility = System.Windows.Visibility.Collapsed;
                accessToken = e.Uri.Fragment.Split('&')[0].Replace("#access_token=", "");
                FBClient = new FacebookClient(accessToken);
                dynamic me = FBClient.Get("me/picture?width=160&height=160&redirect=false");
                Image.Source = new BitmapImage(new Uri(me.data.url));
                WebBrowser.Dispose();
            }*/
        }

        /*private void Button_Click (object sender, RoutedEventArgs e)
        {
            WebBrowser.Visibility = System.Windows.Visibility.Visible;
            var a = AccessToken;
            WebBrowser.Navigate(FBClient.GetLogoutUrl(new { next = "https://www.facebook.com/connect/login_success.html" }));
            var fb = new FacebookClient();

            var logoutUrl = fb.GetLoginUrl(new
            {
                next = "https://www.facebook.com/connect/login_success.html",
                access_token = accessToken
            });
            var webBrowser = new WebBrowser();

            webBrowser.Navigate(logoutUrl.AbsoluteUri);
        }*/

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Button_Check":
                    this.IsHitTestVisible = false;
                    StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
                    {
                        ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                        this.IsHitTestVisible = true;
                    });
                    break;
                default:
                    break;
            }
        }
    }
}