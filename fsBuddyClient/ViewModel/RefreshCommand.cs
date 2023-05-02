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
            var payload = new Dictionary<IRequestPayload, object>();

            payload.Add(IRequestPayload.WATCHER_ID, 1);

            var request = new PipeRequest
            {
                Command = ConnectionInterface.IPipeCommand.GET_WATCHER,
                Payload = payload
            };

            await _serviceConnection.SendPipeRequest(request);           
        }
    }
}
