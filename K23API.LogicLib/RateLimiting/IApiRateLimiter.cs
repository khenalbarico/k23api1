namespace K23API.LogicLib.RateLimiting;

public interface IApiRateLimiter
{
    Task<RateLimitResult1> ConsumeAsync(
        string scopeKey,
        string endpointKey,
        ApiRateLimit1 rateLimit,
        CancellationToken cancellationToken);
}
