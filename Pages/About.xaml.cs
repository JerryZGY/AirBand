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

namespace AirBand.Pages
{
    /// <summary>
    /// About.xaml 的互動邏輯
    /// </summary>
    public partial class About : UserControl
    {
        public About (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
            {
                ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                this.IsHitTestVisible = true;
            });
        }
    }
}
