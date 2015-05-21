using System;
using System.Collections.Generic;
using System.Windows.Controls;
using KinectAirBand.Pages;

namespace KinectAirBand
{
    public static class Switcher
    {
        public static PageSwitcher pageSwitcher;
        public static EnvironmentVariablesViewModel viewModel = new EnvironmentVariablesViewModel();
        public static Dictionary<String,UserControl> PageDictionary = new Dictionary<String,UserControl>()
        {
            {"MainMenu", new MainMenu(viewModel)},
            {"StartPlaying", new StartPlaying(viewModel)},
            {"Setting", new Setting(viewModel)}
        };

        public static void Switch (String switchPage)
        {
            pageSwitcher.Navigate(PageDictionary[switchPage]);
        }

        public static void Switch (UserControl newPage)
        {
            pageSwitcher.Navigate(newPage);
        }

        public static void Switch (UserControl newPage, object state)
        {
            pageSwitcher.Navigate(newPage, state);
        }
    }
}
