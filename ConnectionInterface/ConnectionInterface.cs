using System.Text;
using System.Text.Json;

namespace ConnectionInterface
{
    public enum IPipeCommand
    {
        GET_WATCHER = 0,
        GET_ALL_WATCHER = 1,
        CREATE_WATCHER = 2,
        UPDATE_WATCHER = 3,
        DELETE_WATCHER = 4,
        GET_SERVICESETTING = 5,
        GET_ALL_SERVICESETTING = 6,
        CREATE_SERVICESETTING = 7,
        UPDATE_SERVICESETTING = 8,
        DELETE_SERVICESETTING = 9,
    }

    public class IPayloadWatcher { }

    public class IPayloadServiceSetting { }

    public class PipeRequestPayload
    {
        public int? WatcherId { get; set; }
        public int? ServiceSettingId { get; set; }
        public IPayloadWatcher? WatcherData { get; set; }
        public IPayloadServiceSetting? ServiceSettingData { get; set; }
    }

    public class PipeRequest
    {
        public IPipeCommand Command { get; set; }
        public PipeRequestPayload? Payload { get; set; }
    } 

    public class PipeResponsePayload
    {
        public string? ErrorMessage { get; set; }
    }

    public enum IResponseStatus
    {
        SUCCESS = 0,
        FAILURE = 1,
    }

    public class PipeResponse
    {
        public IResponseStatus Status { get; set; }
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
            await JsonSerializer.SerializeAsync<PipeResponse>(stream, pipeResponse);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static async Task<string> SerializeRequest(PipeRequest pipeRequest)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync<PipeRequest>(stream, pipeRequest);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static PipeResponse ErrorFactory(string error_message)
        {
            return new PipeResponse
            {
                Status = IResponseStatus.FAILURE,
                Payload = new PipeResponsePayload
                {
                    ErrorMessage = error_message,
                },
            };
        }
    }
}