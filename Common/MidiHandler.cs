using System;
using System.ComponentModel;
using NAudio.Midi;

namespace AirBand
{
    public class MidiHandler: IDisposable
    {
        public MidiOut OutDevice;

        public MidiHandler()
        {
            OutDevice = new MidiOut(0);
            Switcher.VM_EnvironmentVariables.PropertyChanged += VM_EnvironmentVariables_PropertyChanged;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (OutDevice != null)
                    {
                        OutDevice.Dispose();
                        OutDevice = null;
                    }
                }

                Switcher.VM_EnvironmentVariables.PropertyChanged -= VM_EnvironmentVariables_PropertyChanged;

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。
                disposedValue = true;
            }
        }

        ~MidiHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void VM_EnvironmentVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OutDeviceIndex" && OutDevice != null)
            {
                OutDevice.Dispose();
                OutDevice = new MidiOut(((VM_EnvironmentVariables)sender).OutDeviceIndex);
            }
        }
    }
}