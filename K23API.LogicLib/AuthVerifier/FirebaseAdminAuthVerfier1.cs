using System.Security.Cryptography;
using System.Text;
using K23API.LogicLib.ApiModels;
using K23API.LogicLib.CentralApi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace K23API.LogicLib.AuthVerifier;

public class FirebaseAdminAuthVerfier1 : IApiAuthVerifier
{
    private static readonly TimeSpan ClockSkew         = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FallbackCacheLife = TimeSpan.FromMinutes(5);

    private readonly string _issuer;
    private readonly string _audience;
    private readonly IMemoryCache _verifiedTokens;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _signingKeys;

    public FirebaseAdminAuthVerfier1(IFirebaseCfg firebaseCfg, IMemoryCache verifiedTokens)
    {
        if (string.IsNullOrWhiteSpace(firebaseCfg.FirebaseProjectId))
            throw new InvalidOperationException("App setting 'FirebaseProjectId' is not set, so Firebase tokens cannot be verified.");

        _issuer         = $"https://securetoken.google.com/{firebaseCfg.FirebaseProjectId}";
        _audience       = firebaseCfg.FirebaseProjectId;
        _verifiedTokens = verifiedTokens;
        _signingKeys    = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{_issuer}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
    }

    public async Task<AuthReq1> VerifyTokenAsync(string? bearerToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bearerToken)) throw ApiEx1.Unauthorized();

        var cacheKey = CacheKeyFor(bearerToken);
        if (_verifiedTokens.TryGetValue<AuthReq1>(cacheKey, out var alreadyVerified) && alreadyVerified is not null)
            return alreadyVerified;

        var verified = await ValidateAgainstFirebaseAsync(bearerToken, cancellationToken);
        _verifiedTokens.Set(cacheKey, verified.Caller, new MemoryCacheEntryOptions { AbsoluteExpiration = verified.ExpiresAt });
        return verified.Caller;
    }

    private async Task<(AuthReq1 Caller, DateTimeOffset ExpiresAt)> ValidateAgainstFirebaseAsync(
        string bearerToken, CancellationToken cancellationToken)
    {
        var openIdConfig = await _signingKeys.GetConfigurationAsync(cancellationToken);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = _issuer,
            ValidAudience            = _audience,
            IssuerSigningKeys        = openIdConfig.SigningKeys,
            ValidAlgorithms          = [SecurityAlgorithms.RsaSha256],
            ClockSkew                = ClockSkew
        };

        var validation = await _tokenHandler.ValidateTokenAsync(bearerToken, validationParameters);
        if (!validation.IsValid || validation.SecurityToken is not JsonWebToken firebaseToken)
            throw ApiEx1.Unauthorized();

        var uid = firebaseToken.Subject;
        if (string.IsNullOrWhiteSpace(uid)) throw ApiEx1.Unauthorized();

        var expiresAt = firebaseToken.ValidTo == default
            ? DateTimeOffset.UtcNow.Add(FallbackCacheLife)
            : new DateTimeOffset(firebaseToken.ValidTo, TimeSpan.Zero);

        return (new AuthReq1 { Uid = uid, BearerToken = bearerToken }, expiresAt);
    }

    private static string CacheKeyFor(string bearerToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(bearerToken)));
}
