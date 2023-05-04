using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client.Model;
using Client.View;
using ConnectionInterface;

namespace Client.ViewModel
{
    public class ShowCreateWatcherCommand : ICommand
    {

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public ShowCreateWatcherCommand() { }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var createWatcherWindow = new CreateWatcher();
            createWatcherWindow.ShowDialog();
        }
    }
}
