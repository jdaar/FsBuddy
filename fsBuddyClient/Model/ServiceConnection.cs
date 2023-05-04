using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using ConnectionInterface;

namespace Client.Model
{
    public class ServiceConnection
    {
        private NamedPipeClientStream? _client;

        private StreamReader? _reader;
        private StreamWriter? _writer;

        public bool IsConnected = false;

        public Action _onPropertyChange;

        public ServiceConnection(Action onPropertyChange)
        {
            _onPropertyChange = onPropertyChange;

            Connect();
        }

        public void Connect()
        {
            _client = new NamedPipeClientStream("fsbuddy-service");

            _reader = new StreamReader(_client);
            _writer = new StreamWriter(_client);

            _client.Connect();
            IsConnected = true;
            _onPropertyChange();
        }
        public void Disconnect()
        {
            _client?.Close();
            IsConnected = false;
            _onPropertyChange();
        }

        ~ServiceConnection()
        {
            _client?.Close();
            _client?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }

        public async Task<PipeResponse?> SendPipeRequest(PipeRequest request)
        {
            if (_client == null)
            {
                throw new Exception("Pipe connection is null");
            }
            if (_writer == null || _reader == null)
            {
                throw new Exception("Pipe connection stream reader or writer is null");
            }

            if (IsConnected == false)
            {
                MessageBox.Show("Pipe is not connected", "Pipe error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            var serializedRequest = await PipeSerializer.SerializeRequest(request);

            await _writer.WriteLineAsync(serializedRequest);
            await _writer.FlushAsync();

            var response = await _reader.ReadLineAsync();

            if (response == null)
            {
                throw new Exception("Response was null");
            }

            var deserializedResponse = await PipeSerializer.DeserializeResponse(response);

            if (deserializedResponse == null)
            {
                throw new Exception("Couldn't deserialize response");
            }

            if (deserializedResponse.Status != IResponseStatus.SUCCESS)
            {
                MessageBox.Show($"Service error: {deserializedResponse.Payload}");
            }

            return deserializedResponse;
        }
    }
}
