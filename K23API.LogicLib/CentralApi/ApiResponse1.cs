namespace K23API.LogicLib.CentralApi;

public class ApiResponse1
{
    public int StatusCode         { get; init; }
    public object? Body           { get; init; }
    public string? AllowedOrigin  { get; init; }
    public int? RetryAfterSeconds { get; init; }

    public bool IsSuccess => StatusCode is >= 200 and < 300;
}
