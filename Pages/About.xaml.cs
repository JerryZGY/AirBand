using System.Windows;
using System.Windows.Controls;

namespace AirBand.Pages
{
    public partial class About : UserControl
    {
        public About (EnvironmentVariablesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            IsHitTestVisible = false;
            StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
            {
                ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                IsHitTestVisible = true;
            });
        }
    }
}
