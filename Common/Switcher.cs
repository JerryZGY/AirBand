using System;
using System.Collections.Generic;
using System.Windows.Controls;
using KinectAirBand.Pages;

namespace KinectAirBand
{
    public static class Switcher
    {
        public static PageSwitcher pageSwitcher;
        public static Dictionary<String,UserControl> PageDictionary = new Dictionary<String,UserControl>()
        {
            {"MainMenu", new MainMenu()},
            {"StartPlaying", new StartPlaying()},
            {"Setting", new Setting()}
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
