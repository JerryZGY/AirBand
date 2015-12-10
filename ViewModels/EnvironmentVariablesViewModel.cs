using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AirBand
{
    public class EnvironmentVariablesViewModel : ViewModelBase
    {
        private Boolean contentEntered = false;
        public Boolean ContentEntered
        {
            get
            {
                return contentEntered;
            }
            set
            {
                contentEntered = value;
                OnPropertyChanged("ContentEntered");
            }
        }

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

        private Double mainVolume = 0;
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

        private Boolean fullScreenToggleButtonEnabled = true;
        public Boolean FullScreenToggleButtonEnabled
        {
            get
            {
                return fullScreenToggleButtonEnabled;
            }
            set
            {
                fullScreenToggleButtonEnabled = value;
                OnPropertyChanged("FullScreenToggleButtonEnabled");
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

        private BitmapImage background = new BitmapImage(new Uri("/AirBand;component/Resources/Background.jpg", UriKind.Relative));
        public BitmapImage Background
        {
            get
            {
                return background;
            }
            set
            {
                background = value;
                OnPropertyChanged("Background");
            }
        }

        private BitmapImage mask = new BitmapImage(new Uri("/AirBand;component/Resources/Mask/Mask00.png", UriKind.Relative));
        public BitmapImage Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
                OnPropertyChanged("Mask");
            }
        }

        private Int32 pianoSense = 1500;
        public Int32 PianoSense
        {
            get
            {
                return pianoSense;
            }
            set
            {
                pianoSense = value;
                OnPropertyChanged("PianoSense");
            }
        }

        private Int32 guitarSense = 500;
        public Int32 GuitarSense
        {
            get
            {
                return guitarSense;
            }
            set
            {
                guitarSense = value;
                OnPropertyChanged("GuitarSense");
            }
        }
    }
}