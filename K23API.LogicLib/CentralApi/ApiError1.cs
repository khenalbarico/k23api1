using System.Text.Json.Serialization;

namespace K23API.LogicLib.CentralApi;

public class ApiError1
{
    [JsonPropertyName("error")]
    public ApiErrorBody1 Error { get; init; } = new();

    public static ApiError1 From(ApiEx1 apiEx, string requestId) => new()
    {
        Error = new ApiErrorBody1
        {
            Code      = apiEx.Code,
            Message   = apiEx.Message,
            RequestId = requestId
        }
    };
}

public class ApiErrorBody1
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = "";

    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    [JsonPropertyName("requestId")]
    public string RequestId { get; init; } = "";
}
