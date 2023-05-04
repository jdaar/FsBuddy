using System;
using System.Collections.Generic;
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
        private NamedPipeClientStream _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        public ServiceConnection()
        {
            _client = new NamedPipeClientStream("fsbuddy-service");

            _reader = new StreamReader(_client);
            _writer = new StreamWriter(_client);

            _client.Connect();
        }

        ~ServiceConnection()
        {
            _client.Close();
            _client.Dispose();
            _reader.Dispose();
            _writer.Dispose();
        }

        public async Task<PipeResponse?> SendPipeRequest(PipeRequest request)
        {
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
