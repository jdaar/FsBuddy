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
    public class EditWatcherCommand : ICommand
    {
        private readonly ServiceConnection _serviceConnection;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public EditWatcherCommand(ServiceConnection serviceConnection)
        {
            _serviceConnection = serviceConnection;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var watcherForm = (t_WatcherForm)parameter;

            var request = new PipeRequest
            {
                Command = t_PipeCommand.UPDATE_WATCHER,
                Payload = new PipeRequestPayload
                {
                    WatcherId = watcherForm.Id,
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

            await _serviceConnection.RefreshWatchers();

            MessageBox.Show($"Status: {response?.Status}");
        }
    }
}
