using K23API.LogicLib.ApiModels;
using K23API.LogicLib.CentralApi;
using K23API.LogicLib.RateLimiting;

namespace K23API.LogicLib.Apps.DocumentManager;

public interface IDocumentManager
{
    [ApiMethod1(RequiresAuth = true, RateLimit = ApiRateLimitTier1.Standard)]
    Task<DocumentManagerResp1> CreateUploadUrl(DocumentManagerReq1 request, ApiCall1 call);

    [ApiMethod1(RequiresAuth = true, RateLimit = ApiRateLimitTier1.Strict)]
    Task<DocumentManagerResp1> ConvertToPdf(DocumentManagerReq1 request, ApiCall1 call, CancellationToken cancellationToken);
}
