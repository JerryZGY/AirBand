using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectAirBand
{
    public class EnvironmentVariablesViewModel : INotifyPropertyChanged
    {
        private Boolean mainVolumeMute = false;
        private Double mainVolume = 0.5;
        private Boolean fullScreen = false;
        private Boolean backgroundRemoval = false;
        public event PropertyChangedEventHandler PropertyChanged;

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
                Switcher.pageSwitcher.WindowState = (fullScreen) ? WindowState.Maximized : WindowState.Normal;
                OnPropertyChanged("FullScreen");
            }
        }

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

        protected virtual void OnPropertyChanged (String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
