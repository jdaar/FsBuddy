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
    public class CreateWatcherCommand : ICommand
    {
        private readonly ServiceConnection _serviceConnection;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public CreateWatcherCommand(ServiceConnection serviceConnection)
        {
            _serviceConnection = serviceConnection;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var watcherForm = (WatcherForm)parameter;

            var request = new PipeRequest
            {
                Command = t_PipeCommand.CREATE_WATCHER,
                Payload = new PipeRequestPayload
                {
                    WatcherData = new Watcher
                    {
                        Name = watcherForm.Name,
                        InputPath = watcherForm.InputPath,
                        OutputPath = watcherForm.OutputPath,
                        SearchPattern = watcherForm.Filter,
                        Action = t_WatcherAction.MOVE
                    }
                }
            };

           var response = await _serviceConnection.SendPipeRequest(request);

            if (response == null)
            {
                MessageBox.Show("Couldn't retrieve service response", "Pipe error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Status: {response?.Status}");
        }
    }
}
