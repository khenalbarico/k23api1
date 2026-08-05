namespace K23API.LogicLib.RateLimiting;

public class RateLimitResult1
{
    public bool IsAllowed         { get; init; }
    public int RetryAfterSeconds  { get; init; }

    public static RateLimitResult1 Allowed => new() { IsAllowed = true };

    public static RateLimitResult1 Blocked(int retryAfterSeconds) =>
        new() { IsAllowed = false, RetryAfterSeconds = Math.Max(1, retryAfterSeconds) };
}
