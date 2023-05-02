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

    public enum IRequestPayload
    {
        WATCHER_ID = 0,
        SERVICESETTING_ID = 1,
        WATCHER_DATA = 2,
        SERVICESETTING_DATA = 3,
    }

    public class PipeRequest
    {
        public IPipeCommand Command { get; set; }
        public Dictionary<IRequestPayload, object> Payload { get; set; }
    } 

    public enum IResponsePayload
    {
        ERROR_MESSAGE = 0,
    }

    public enum IResponseStatus
    {
        SUCCESS = 0,
        FAILURE = 1,
    }

    public class PipeResponse
    {
        public IResponseStatus Status { get; set; }
        public Dictionary<IResponsePayload, object> Payload { get; set; }
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
            var response = JsonSerializer.Serialize<PipeResponse>(pipeResponse);
            return response;
        }

        public static async Task<string> SerializeRequest(PipeRequest pipeRequest)
        {
            var response = JsonSerializer.Serialize<PipeRequest>(pipeRequest);
            return response;
        }

        public static async Task<PipeResponse> ErrorFactory(string error_message)
        {
            var payload = new Dictionary<IResponsePayload, object>();
            payload.Add(IResponsePayload.ERROR_MESSAGE, error_message);
            return new PipeResponse
            {
                Status = IResponseStatus.FAILURE,
                Payload = payload,
            };
        }
    }
}