namespace K23API.LogicLib.CentralApi;

public class ApiHealthEndpoint1 : IApiEndpoint
{
    public string ApiClassName  => "IApiHealth";
    public string ApiMethodName => "Ping";
    public bool RequiresAuth    => false;

    public Task<object?> HandleAsync(ApiCall1 call, CancellationToken cancellationToken) =>
        Task.FromResult<object?>(new { status = "ok", utc = DateTimeOffset.UtcNow });
}
