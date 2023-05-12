using System.Text;
using System.Text.Json;

namespace ConnectionInterface
{
    public enum PipeCommand : int
    {
        GET_WATCHER = 0,
        GET_ALL_WATCHER = 1,
        GET_WATCHER_STATUS = 2,
        CREATE_WATCHER = 3,
        UPDATE_WATCHER = 4,
        DELETE_WATCHER = 5,
        PAUSE_WATCHER = 6,
        START_WATCHER = 7,
    }

    public enum ResponseStatus
    {
        SUCCESS = 0,
        FAILURE = 1,
    }

    public class PipeRequestPayload
    {
        public int? WatcherId { get; set; }
        public Watcher? WatcherData { get; set; }
    }

    public class PipeRequest
    {
        public PipeCommand Command { get; set; }
        public PipeRequestPayload? Payload { get; set; }
    } 

    public class PipeResponsePayload
    {
        public string? ErrorMessage { get; set; }
        public List<Watcher>? Watchers { get; set; }
        public bool? WatcherStatus { get; set; }
    }

    public class PipeResponse
    {
        public ResponseStatus Status { get; set; }
        public PipeResponsePayload? Payload { get; set; }
    }

    public class PipeSerializer
    {
        public static async Task<PipeRequest?> DeserializeRequest(string request)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
            return await JsonSerializer.DeserializeAsync<PipeRequest>(stream);
        } 

        public static async Task<PipeResponse?> DeserializeResponse(string response)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
            return await JsonSerializer.DeserializeAsync<PipeResponse>(stream);
        } 

        public static async Task<string> SerializeResponse(PipeResponse pipeResponse)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, pipeResponse);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static async Task<string> SerializeRequest(PipeRequest pipeRequest)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, pipeRequest);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static PipeResponse ErrorFactory(string error_message)
        {
            return new PipeResponse
            {
                Status = ResponseStatus.FAILURE,
                Payload = new PipeResponsePayload
                {
                    ErrorMessage = error_message,
                },
            };
        }
    }
}