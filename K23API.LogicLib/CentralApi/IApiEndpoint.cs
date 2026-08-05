namespace K23API.LogicLib.CentralApi;

public interface IApiEndpoint
{
    string ApiClassName  { get; }
    string ApiMethodName { get; }
    bool RequiresAuth    { get; }

    Task<object?> HandleAsync(ApiCall1 call, CancellationToken cancellationToken);
}
