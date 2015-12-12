using System;

namespace AirBand
{
    public interface ISwitchable
    {
        void InitializeProperty();
        void EnterStory();
        void ExitStory(Action callback);
    }
}