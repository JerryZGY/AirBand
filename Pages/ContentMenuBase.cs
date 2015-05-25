using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KinectAirBand.Pages
{
    public class ContentMenuBase
    {
        protected void Check (UserControl content)
        {
            content.IsHitTestVisible = false;
            StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
            {
                ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                Switcher.viewModel.ContentEntered = false;
                content.IsHitTestVisible = true;
            });
        }
    }
}
