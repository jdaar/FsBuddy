using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client.Model;
using ConnectionInterface;

namespace Client.ViewModel
{
    public class SwitchConnectionStatus : ICommand
    {
        private readonly ServiceConnection _serviceConnection;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public SwitchConnectionStatus(ServiceConnection serviceConnection)
        {
            _serviceConnection = serviceConnection;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (_serviceConnection.IsConnected)
            {
                _serviceConnection.Disconnect();
            }
            else
            {
                _serviceConnection.Connect();
            }
        }
    }
}
