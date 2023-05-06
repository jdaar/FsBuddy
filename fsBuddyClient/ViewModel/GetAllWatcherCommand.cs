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
    public class GetAllWatcherCommand : ICommand
    {
        private readonly ServiceConnection _serviceConnection;
        private readonly Presenter _presenter;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public GetAllWatcherCommand(ServiceConnection serviceConnection, Presenter presenter)
        {
            _serviceConnection = serviceConnection;
            _presenter = presenter;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await _serviceConnection.RefreshWatchers();
        }
    }
}
