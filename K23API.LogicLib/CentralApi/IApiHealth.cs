using K23API.LogicLib.RateLimiting;

namespace K23API.LogicLib.CentralApi;

public interface IApiHealth
{
    [ApiMethod1(RequiresAuth = false, RateLimit = ApiRateLimitTier1.ReadOnly)]
    Task<object> Ping();
}
