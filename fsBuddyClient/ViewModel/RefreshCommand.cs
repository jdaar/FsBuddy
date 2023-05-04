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
    public class RefreshCommand : ICommand
    {
        private readonly ServiceConnection _serviceConnection;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public RefreshCommand(ServiceConnection serviceConnection)
        {
            _serviceConnection = serviceConnection;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var request = new PipeRequest
            {
                Command = IPipeCommand.GET_WATCHER,
                Payload = new PipeRequestPayload
                {
                    WatcherId = 1
                }
            };

           var response = await _serviceConnection.SendPipeRequest(request);

            MessageBox.Show($"Status: {response?.Status}. Payload: {response?.Payload?.ErrorMessage}");
        }
    }
}
