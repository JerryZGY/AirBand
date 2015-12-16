using System;
using System.Windows.Input;

namespace AirBand
{
    public class RightClickCommand : ICommand
    {
        Action action;
        public event EventHandler CanExecuteChanged;
        public RightClickCommand(Action action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return Switcher.VM_EnvironmentVariables.ContentEntered;
        }

        public void Execute(object parameter)
        {
            action();
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }
    }
}