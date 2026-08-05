namespace K23API.LogicLib.RateLimiting;

public enum ApiRateLimitTier1
{
    Strict,
    Standard,
    ReadOnly
}

public static class ApiRateLimitTiers1
{
    public static ApiRateLimit1 Resolve(ApiRateLimitTier1 tier) => tier switch
    {
        ApiRateLimitTier1.ReadOnly => ApiRateLimit1.ReadOnly,
        ApiRateLimitTier1.Standard => ApiRateLimit1.Standard,
        _                          => ApiRateLimit1.Strict
    };
}
