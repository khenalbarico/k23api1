namespace K23API.LogicLib.CentralApi;

public class ApiHealth1 : IApiHealth
{
    public Task<object> Ping() =>
        Task.FromResult<object>(new { status = "ok", utc = DateTimeOffset.UtcNow });
}
