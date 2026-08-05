namespace K23API.LogicLib.CentralApi;

public class ApiEx1(string code, int statusCode, string message) : Exception(message)
{
    public string Code    { get; } = code;
    public int StatusCode { get; } = statusCode;

    public static ApiEx1 BadRequest(string message)  => new("bad_request", 400, message);
    public static ApiEx1 Unauthorized()              => new("unauthorized", 401, "Your session is invalid or has expired. Please sign in again.");
    public static ApiEx1 OriginNotAllowed()          => new("origin_not_allowed", 403, "This origin is not allowed to call the API.");
    public static ApiEx1 EndpointNotFound()          => new("endpoint_not_found", 404, "The requested API endpoint does not exist.");
    public static ApiEx1 Unexpected()                => new("internal_error", 500, "Something went wrong while handling the request.");
}
