using Microsoft.Extensions.Logging;

namespace K23API.LogicLib.CentralApi;

public class ApiDispatcher1(
    IEnumerable<IApiEndpoint> endpoints,
    ApiGate1 apiGate,
    ILogger<ApiDispatcher1> logger)
{
    private readonly IReadOnlyDictionary<string, IApiEndpoint> _endpointsByName = endpoints.ToDictionary(
        endpoint => EndpointKey(endpoint.ApiClassName, endpoint.ApiMethodName),
        StringComparer.OrdinalIgnoreCase);

    public async Task<ApiResponse1> DispatchAsync(ApiRequest1 request, CancellationToken cancellationToken)
    {
        var allowedOrigin = apiGate.ResolveAllowedOrigin(request.Origin);

        try
        {
            apiGate.EnsureOriginAllowed(request.Origin);

            var endpoint = FindEndpoint(request);
            var caller   = endpoint.RequiresAuth
                ? await apiGate.VerifyCallerAsync(request.AuthorizationHeader, cancellationToken)
                : null;

            var call = new ApiCall1
            {
                ApiClassName  = endpoint.ApiClassName,
                ApiMethodName = endpoint.ApiMethodName,
                PayloadJson   = request.PayloadJson,
                Caller        = caller
            };

            var result = await endpoint.HandleAsync(call, cancellationToken);
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

    private IApiEndpoint FindEndpoint(ApiRequest1 request) =>
        _endpointsByName.TryGetValue(EndpointKey(request.ApiClassName, request.ApiMethodName), out var endpoint)
            ? endpoint
            : throw ApiEx1.EndpointNotFound();

    private ApiResponse1 Failure(ApiEx1 apiEx, ApiRequest1 request, string? allowedOrigin)
    {
        logger.LogWarning("Rejected {ApiClassName}.{ApiMethodName} with {Code} (requestId {RequestId})",
            request.ApiClassName, request.ApiMethodName, apiEx.Code, request.RequestId);

        return new ApiResponse1
        {
            StatusCode    = apiEx.StatusCode,
            Body          = ApiError1.From(apiEx, request.RequestId),
            AllowedOrigin = allowedOrigin
        };
    }

    private static string EndpointKey(string apiClassName, string apiMethodName) => $"{apiClassName}/{apiMethodName}";
}
