using System;
using System.Collections.Generic;
using System.Windows.Controls;
using AirBand.Pages;

namespace AirBand
{
    public static class Switcher
    {
        public static PageSwitcher pageSwitcher;
        public static EnvironmentVariablesViewModel viewModel = new EnvironmentVariablesViewModel();
        public static Dictionary<String,UserControl> PageDictionary = new Dictionary<String,UserControl>()
        {
            {"MainMenu", new MainMenu(viewModel)},
            {"Playing", new Playing(viewModel)},
            {"Community", new Community(viewModel)},
            {"Mod", new Mod(viewModel)},
            {"Setting", new Setting(viewModel)},
            {"About", new About(viewModel)},
            {"BRPlaying", new BRPlaying(viewModel)}
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