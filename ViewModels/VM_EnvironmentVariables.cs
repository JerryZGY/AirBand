using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AirBand
{
    public class VM_EnvironmentVariables : ViewModelBase
    {
        private bool contentEntered = false;
        public bool ContentEntered
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

        private bool mainVolumeMute = false;
        public bool MainVolumeMute
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

        private double mainVolume = .5;
        public double MainVolume
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

        private bool fullScreen = false;
        public bool FullScreen
        {
            get
            {
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

        private bool fullScreenToggleButtonEnabled = true;
        public bool FullScreenToggleButtonEnabled
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

        private bool backgroundRemoval = false;
        public bool BackgroundRemoval
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

        private int backgroundIndex = 0;
        public int BackgroundIndex
        {
            get
            {
                return backgroundIndex;
            }
            set
            {
                backgroundIndex = value;
                OnPropertyChanged("BackgroundIndex");
            }
        }

        private BitmapImage background = new BitmapImage(new Uri("/AirBand;component/Materials/Background.jpg", UriKind.Relative));
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

        private int maskIndex = 0;
        public int MaskIndex
        {
            get
            {
                return maskIndex;
            }
            set
            {
                maskIndex = value;
                OnPropertyChanged("MaskIndex");
            }
        }

        private BitmapImage mask = new BitmapImage(new Uri("/AirBand;component/Materials/Masks/Mask00.png", UriKind.Relative));
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

        private int instrumentsVolume = 63;
        public int InstrumentsVolume
        {
            get
            {
                return instrumentsVolume;
            }
            set
            {
                instrumentsVolume = value;
                OnPropertyChanged("InstrumentsVolume");
            }
        }

        private int pianoSense = 1500;
        public int PianoSense
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

        private int guitarSense = 500;
        public int GuitarSense
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