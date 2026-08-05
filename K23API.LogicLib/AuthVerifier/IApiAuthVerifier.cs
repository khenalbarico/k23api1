using K23API.LogicLib.ApiModels;

namespace K23API.LogicLib.AuthVerifier;

public interface IApiAuthVerifier
{
    Task<AuthReq1> VerifyTokenAsync(string? bearerToken, CancellationToken cancellationToken = default);
}
