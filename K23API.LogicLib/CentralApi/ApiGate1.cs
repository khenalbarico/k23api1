using K23API.LogicLib.ApiModels;
using K23API.LogicLib.AuthVerifier;

namespace K23API.LogicLib.CentralApi;

public class ApiGate1(IApiGateCfg gateCfg, IApiAuthVerifier authVerifier)
{
    private const string BearerPrefix = "Bearer ";

    public bool HasOriginAllowlist => gateCfg.AllowedOrigins.Count > 0;

    public string? ResolveAllowedOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin)) return null;

        return gateCfg.AllowedOrigins.FirstOrDefault(
            allowed => string.Equals(allowed, origin, StringComparison.OrdinalIgnoreCase));
    }

    public void EnsureOriginAllowed(string? origin)
    {
        if (!HasOriginAllowlist) return;
        if (string.IsNullOrWhiteSpace(origin)) return;
        if (ResolveAllowedOrigin(origin) is null) throw ApiEx1.OriginNotAllowed();
    }

    public Task<AuthReq1> VerifyCallerAsync(string? authorizationHeader, CancellationToken cancellationToken) =>
        authVerifier.VerifyTokenAsync(ReadBearerToken(authorizationHeader), cancellationToken);

    private static string? ReadBearerToken(string? authorizationHeader) =>
        authorizationHeader is not null && authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[BearerPrefix.Length..].Trim()
            : null;
}
