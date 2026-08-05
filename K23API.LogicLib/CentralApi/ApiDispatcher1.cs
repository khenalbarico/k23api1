using K23API.LogicLib.ApiModels;
using K23API.LogicLib.RateLimiting;
using Microsoft.Extensions.Logging;

namespace K23API.LogicLib.CentralApi;

public class ApiDispatcher1(
    ApiClassRegistry1 apiClassRegistry,
    ApiMethodInvoker1 apiMethodInvoker,
    ApiGate1 apiGate,
    IApiRateLimiter rateLimiter,
    ILogger<ApiDispatcher1> logger)
{
    private const string UnknownCallerScope = "anonymous-unknown";

    public async Task<ApiResponse1> DispatchAsync(ApiRequest1 request, CancellationToken cancellationToken)
    {
        var allowedOrigin = apiGate.ResolveAllowedOrigin(request.Origin);

        try
        {
            apiGate.EnsureOriginAllowed(request.Origin);

            var apiMethod = apiClassRegistry.Find(request.ApiClassName, request.ApiMethodName)
                            ?? throw ApiEx1.EndpointNotFound();

            var caller = apiMethod.RequiresAuth
                ? await apiGate.VerifyCallerAsync(request.AuthorizationHeader, cancellationToken)
                : null;

            await EnsureWithinRateLimitAsync(apiMethod, caller, request, cancellationToken);

            var call = new ApiCall1
            {
                ApiClassName  = apiMethod.ApiClassName,
                ApiMethodName = apiMethod.ApiMethodName,
                PayloadJson   = request.PayloadJson,
                Caller        = caller
            };

            var result = await apiMethodInvoker.InvokeAsync(apiMethod, call, cancellationToken);
            return new ApiResponse1 { StatusCode = 200, Body = result, AllowedOrigin = allowedOrigin };
        }
        catch (ApiEx1 apiEx)
        {
            return Failure(apiEx, request, allowedOrigin);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled failure dispatching {ApiClassName}.{ApiMethodName} (requestId {RequestId})",
                request.ApiClassName, request.ApiMethodName, request.RequestId);

            return Failure(ApiEx1.Unexpected(), request, allowedOrigin);
        }
    }

    private async Task EnsureWithinRateLimitAsync(
        ApiMethodDescriptor1 apiMethod, AuthReq1? caller, ApiRequest1 request, CancellationToken cancellationToken)
    {
        var consumed = await rateLimiter.ConsumeAsync(
            ScopeKeyFor(caller, request),
            $"{apiMethod.ApiClassName}-{apiMethod.ApiMethodName}",
            apiMethod.RateLimit,
            cancellationToken);

        if (!consumed.IsAllowed) throw ApiEx1.RateLimited(consumed.RetryAfterSeconds);
    }

    private static string ScopeKeyFor(AuthReq1? caller, ApiRequest1 request)
    {
        if (caller is not null && !string.IsNullOrWhiteSpace(caller.Uid)) return $"uid-{caller.Uid}";

        return string.IsNullOrWhiteSpace(request.ClientIp) ? UnknownCallerScope : $"ip-{request.ClientIp}";
    }

    private ApiResponse1 Failure(ApiEx1 apiEx, ApiRequest1 request, string? allowedOrigin)
    {
        logger.LogWarning("Rejected {ApiClassName}.{ApiMethodName} with {Code} (requestId {RequestId})",
            request.ApiClassName, request.ApiMethodName, apiEx.Code, request.RequestId);

        return new ApiResponse1
        {
            StatusCode        = apiEx.StatusCode,
            Body              = ApiError1.From(apiEx, request.RequestId),
            AllowedOrigin     = allowedOrigin,
            RetryAfterSeconds = apiEx.RetryAfterSeconds
        };
    }
}
