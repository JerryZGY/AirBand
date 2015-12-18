using System;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;

namespace AirBand
{
    public class MyoHandler
    {
        public delegate void MyoInputHandler();
        public event MyoInputHandler InputHandler;
        public MyoEventArgs MyoEventArgs;
        private IHeldPose instSelectorPose;

        public MyoHandler()
        {
            var channel = Channel.Create(
            ChannelDriver.Create(ChannelBridge.Create(),
            MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            var hub = Hub.Create(channel);
            hub.MyoConnected += (sender, e) =>
            {
                MyoEventArgs = e;
            };
            channel.StartListening();
        }

        public void StartInstSelectorPose()
        {
            if (MyoEventArgs == null)
                return;
            if (!MyoEventArgs.Myo.IsUnlocked)
                MyoEventArgs.Myo.Unlock(UnlockType.Hold);
            instSelectorPose = HeldPose.Create(MyoEventArgs.Myo, Pose.DoubleTap);
            instSelectorPose.Triggered += Pose_Triggered;
            instSelectorPose.Interval = TimeSpan.FromSeconds(.5);
            instSelectorPose.Start();
        }

        public void StopInstSelectorPose()
        {
            instSelectorPose.Triggered -= Pose_Triggered;
            instSelectorPose.Stop();
            instSelectorPose.Dispose();
            instSelectorPose = null;
            MyoEventArgs.Myo.Lock();
        }

        private void Pose_Triggered(object sender, PoseEventArgs e)
        {
            if (InputHandler != null)
                InputHandler();
        }
    }
}