namespace K23API.LogicLib.CentralApi;

public class ApiRequest1
{
    public string ApiClassName        { get; init; } = "";
    public string ApiMethodName       { get; init; } = "";
    public string? PayloadJson        { get; init; }
    public string? Origin             { get; init; }
    public string? AuthorizationHeader { get; init; }
    public string RequestId           { get; init; } = "";
}
