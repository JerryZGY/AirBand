using System;
using System.Media;
using System.Windows;

namespace AirBand
{
    public class SoundHandler
    {
        private SoundPlayer snd;
        private Uri cheerUri = new Uri("/AirBand;component/Sounds/Cheer.wav", UriKind.Relative);
        private Uri gloomUri = new Uri("/AirBand;component/Sounds/Gloom.wav", UriKind.Relative);

        public SoundHandler()
        {
            snd = new SoundPlayer();
        }

        public void PlayCheerSound()
        {
            snd.Stream = Application.GetResourceStream(cheerUri).Stream;
            snd.Play();
        }

        public void PlayGloomSound()
        {
            snd.Stream = Application.GetResourceStream(gloomUri).Stream;
            snd.Play();
        }
    }
}