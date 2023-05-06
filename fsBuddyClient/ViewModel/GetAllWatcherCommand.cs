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
            var request = new PipeRequest
            {
                Command = t_PipeCommand.GET_ALL_WATCHER,
                Payload = new PipeRequestPayload { }
            };

           var response = await _serviceConnection.SendPipeRequest(request);

            if (response == null)
            {
                MessageBox.Show("Couldn't retrieve service response", "Pipe error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Status: {response?.Status}");
            MessageBox.Show($"Status: {String.Join(',', response?.Payload.Watchers.Select(v => v.Name))}");

            if (response?.Payload?.Watchers == null)
            {
                MessageBox.Show("Couldn't retrieve watchers", "Pipe error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _presenter.Watchers = response.Payload.Watchers;
        }
    }
}
