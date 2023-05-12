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
    public class PauseWatcherCommand : ICommand
    {
        private readonly ServiceConnection _serviceConnection;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public PauseWatcherCommand(ServiceConnection serviceConnection)
        {
            _serviceConnection = serviceConnection;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var watcher = (Watcher)parameter;
            watcher.IsEnabled = !watcher.IsEnabled;

            var request = new PipeRequest
            {
                Command = PipeCommand.UPDATE_WATCHER,
                Payload = new PipeRequestPayload
                {
                    WatcherId = watcher.Id,
                    WatcherData = watcher
                }
            };

           var response = await _serviceConnection.SendPipeRequest(request);

            if (response == null)
            {
                MessageBox.Show("Couldn't retrieve service response", "Pipe error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _serviceConnection.RefreshWatchers();

            if (response?.Status == ResponseStatus.SUCCESS)
            {
                MessageBox.Show("Watcher was succesfully paused", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
