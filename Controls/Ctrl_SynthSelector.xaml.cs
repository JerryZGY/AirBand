using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NAudio.Midi;

namespace AirBand.Controls
{
    public partial class Ctrl_SynthSelector : UserControl
    {
        private int index = Switcher.VM_EnvironmentVariables.OutDeviceIndex;
        private List<string> outDevices;

        public Ctrl_SynthSelector()
        {
            InitializeComponent();
        }

        private void ctrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                outDevices = new List<string>();
                for (var device = 0; device < MidiOut.NumberOfDevices; device++)
                    outDevices.Add(MidiOut.DeviceInfo(device).ProductName);
                if (outDevices.Count != 0)
                    Lbl.Text = outDevices[index];
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (outDevices.Count == 0)
                return;
            var button = sender as Button;
            switch (button.Name)
            {
                case "Btn_Prev":
                    index = (index > 0) ? (index - 1) : (outDevices.Count - 1);
                    break;
                case "Btn_Next":
                    index = (index < outDevices.Count - 1) ? (index + 1) : 0;
                    break;
            }
            Lbl.Text = outDevices[index];
            Switcher.VM_EnvironmentVariables.OutDeviceIndex = index;
        }
    }
}