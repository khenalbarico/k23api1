using K23API.LogicLib.RateLimiting;

namespace K23API.LogicLib.CentralApi;

[AttributeUsage(AttributeTargets.Method)]
public class ApiMethod1 : Attribute
{
    public bool RequiresAuth            { get; init; } = true;
    public ApiRateLimitTier1 RateLimit  { get; init; } = ApiRateLimitTier1.Strict;
}
