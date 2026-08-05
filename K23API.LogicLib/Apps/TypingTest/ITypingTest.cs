using K23API.LogicLib.ApiModels;
using K23API.LogicLib.CentralApi;
using K23API.LogicLib.RateLimiting;

namespace K23API.LogicLib.Apps.TypingTest;

public interface ITypingTest
{
    [ApiMethod1(RequiresAuth = true, RateLimit = ApiRateLimitTier1.Standard)]
    Task<TypingTestResp1> LoadWebGL(CancellationToken cancellationToken);
}
