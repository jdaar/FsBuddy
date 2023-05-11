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
    public class ShowEditWatcherCommand : ICommand
    {

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public ShowEditWatcherCommand() { }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Watcher watcher = (Watcher)parameter;

            var createWatcherWindow = new EditWatcher(watcher.Id);
            createWatcherWindow.ShowDialog();
        }
    }
}
