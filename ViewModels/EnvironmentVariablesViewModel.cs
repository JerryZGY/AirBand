using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace KinectAirBand
{
    public class EnvironmentVariablesViewModel : ViewModelBase
    {

        private Boolean mainVolumeMute = false;
        public Boolean MainVolumeMute
        {
            get
            {
                return mainVolumeMute;
            }
            set
            {
                mainVolumeMute = value;
                OnPropertyChanged("MainVolumeMute");
            }
        }

        private Double mainVolume = 0.1;
        public Double MainVolume
        {
            get
            {
                return mainVolume;
            }
            set
            {
                mainVolume = value;
                OnPropertyChanged("MainVolume");
            }
        }

        private Boolean fullScreen = false;
        public Boolean FullScreen
        {
            get
            {
                Switcher.pageSwitcher.Left = 0;
                Switcher.pageSwitcher.Top = 0;
                return fullScreen;
            }
            set
            {
                fullScreen = value;
                Switcher.pageSwitcher.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Switcher.pageSwitcher.WindowState = ( fullScreen ) ? WindowState.Maximized : WindowState.Normal;
                OnPropertyChanged("FullScreen");
            }
        }

        private Boolean backgroundRemoval = false;
        public Boolean BackgroundRemoval
        {
            get
            {
                return backgroundRemoval;
            }
            set
            {
                backgroundRemoval = value;
                OnPropertyChanged("BackgroundRemoval");
            }
        }

        private Int32 background = 0;
        public String Background
        {
            get
            {
                var test = System.IO.Directory.GetFileSystemEntries(@"Resources/Background", "*.jpg")
                   .Select((index, value) => new { index, value })
                   .ToDictionary(item => item.value, item =>
                   {
                       return String.Format("../{0}", item.index);
                   })[background];
                return test;
            }
            set
            {
                background = Convert.ToInt32(value);
                OnPropertyChanged("Background");
            }
        }
    }
}