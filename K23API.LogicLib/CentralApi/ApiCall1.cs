using System.Text.Json;
using K23API.LogicLib.ApiModels;

namespace K23API.LogicLib.CentralApi;

public class ApiCall1
{
    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public string ApiClassName  { get; init; } = "";
    public string ApiMethodName { get; init; } = "";
    public string? PayloadJson  { get; init; }
    public AuthReq1? Caller     { get; init; }

    public string CallerUid => Caller?.Uid ?? "";

    public T PayloadAs<T>()
    {
        if (string.IsNullOrWhiteSpace(PayloadJson))
            throw ApiEx1.BadRequest($"'{ApiClassName}.{ApiMethodName}' requires a payload but none was supplied.");

        try
        {
            return JsonSerializer.Deserialize<T>(PayloadJson, PayloadOptions)
                   ?? throw ApiEx1.BadRequest($"'{ApiClassName}.{ApiMethodName}' received an empty payload.");
        }
        catch (JsonException)
        {
            throw ApiEx1.BadRequest($"'{ApiClassName}.{ApiMethodName}' received a payload that is not valid JSON.");
        }
    }
}
