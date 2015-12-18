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
        private IHeldPose instSelectorPose;
        private MyoEventArgs myoEventArgs;

        public MyoHandler()
        {
            var channel = Channel.Create(
            ChannelDriver.Create(ChannelBridge.Create(),
            MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            var hub = Hub.Create(channel);
            hub.MyoConnected += (sender, e) =>
            {
                myoEventArgs = e;
                myoEventArgs.Myo.Unlock(UnlockType.Hold);
                StartInstSelectorPose();
            };
            channel.StartListening();
        }

        public void StartInstSelectorPose()
        {
            instSelectorPose = HeldPose.Create(myoEventArgs.Myo, Pose.DoubleTap);
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
        }

        private void Pose_Triggered(object sender, PoseEventArgs e)
        {
            if (InputHandler != null)
                InputHandler();
        }
    }
}