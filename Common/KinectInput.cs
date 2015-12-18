using System;
using System.Windows;

namespace AirBand
{
    public delegate void KinectInputEventHandler(KinectInputArgs e);

    public enum InputState
    {
        Close = 0, Open = 1
    }

    public class KinectInputArgs : EventArgs
    {
        public Point Posotion { get; }

        public InputState InputState { get; }

        public bool IsValid { get; }

        public KinectInputArgs(Point posotion, InputState state, bool isValid)
        {
            Posotion = posotion;
            InputState = state;
            IsValid = isValid;
        }
    }
}