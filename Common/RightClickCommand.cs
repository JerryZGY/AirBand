using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KinectAirBand
{
    public class RightClickCommand : ICommand
    {
        Action action;
        public event EventHandler CanExecuteChanged;
        public RightClickCommand (Action action)
        {
            this.action = action;
        }

        public bool CanExecute (object parameter)
        {
            return Switcher.viewModel.ContentEntered;
        }

        public void Execute (object parameter)
        {
            action();
        }
    }
}
