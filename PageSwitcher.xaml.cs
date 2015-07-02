using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AirBand
{
    public partial class PageSwitcher : Window
    {
        public PageSwitcher ()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            Switcher.viewModel.FullScreen = !( SystemParameters.FullPrimaryScreenWidth > 1366 );
            Switcher.viewModel.FullScreenToggleButtonEnabled = ( SystemParameters.FullPrimaryScreenWidth > 1366 );
            Control.IsTabStopProperty.OverrideMetadata(typeof(Control), new FrameworkPropertyMetadata(false));
            this.InputBindings.Add(new InputBinding(RightClickCommand(), new MouseGesture(MouseAction.RightClick)));
            Switcher.Switch("MainMenu");
        }

        private ICommand RightClickCommand ()
        {
            return new RightClickCommand(() =>
            {
                this.IsHitTestVisible = false;
                StoryboardHandler.InitNotHitStoryBoard(Switcher.PageDictionary["MainMenu"], "ExitContentStoryboard", () =>
                {
                    ( (Grid)Switcher.PageDictionary["MainMenu"].FindName("Grid_Main") ).IsHitTestVisible = true;
                    Switcher.viewModel.ContentEntered = false;
                    this.IsHitTestVisible = true;
                });
            });
        }

        public void Navigate (UserControl nextPage)
        {
            this.Content = nextPage;
        }

        public void Navigate (UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;
            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name.ToString());
        }
    }
}