namespace K23API.LogicLib.RateLimiting;

public class ApiRateLimit1(int maxRequests, TimeSpan window)
{
    public int MaxRequests  { get; } = maxRequests;
    public TimeSpan Window  { get; } = window;

    public static ApiRateLimit1 ReadOnly => new(120, TimeSpan.FromMinutes(1));
    public static ApiRateLimit1 Standard => new(30, TimeSpan.FromMinutes(1));
    public static ApiRateLimit1 Strict   => new(10, TimeSpan.FromMinutes(1));
}
