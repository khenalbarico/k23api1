using K23API.LogicLib.CentralApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace K23API;

public class ApiRelayer1(ApiDispatcher1 apiDispatcher, ApiGate1 apiGate)
{
    private const string PayloadQueryKey = "payload";

    [Function("ApiRelayer1")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "options", Route = "v1/{apiClassName}/{apiMethodName}")]
        HttpRequest req,
        string apiClassName,
        string apiMethodName,
        CancellationToken cancellationToken)
    {
        var origin = req.Headers.Origin.FirstOrDefault();

        if (HttpMethods.IsOptions(req.Method)) return Preflight(req, origin);

        var apiRequest = new ApiRequest1
        {
            ApiClassName        = apiClassName,
            ApiMethodName       = apiMethodName,
            PayloadJson         = await ReadPayloadAsync(req),
            Origin              = origin,
            AuthorizationHeader = req.Headers.Authorization.FirstOrDefault(),
            RequestId           = req.HttpContext.TraceIdentifier
        };

        var apiResponse = await apiDispatcher.DispatchAsync(apiRequest, cancellationToken);
        ApplyCorsHeaders(req, apiResponse.AllowedOrigin);

        return new ObjectResult(apiResponse.Body) { StatusCode = apiResponse.StatusCode };
    }

    private IActionResult Preflight(HttpRequest req, string? origin)
    {
        var allowedOrigin = apiGate.ResolveAllowedOrigin(origin);
        if (apiGate.HasOriginAllowlist && allowedOrigin is null) return new StatusCodeResult(StatusCodes.Status403Forbidden);

        ApplyCorsHeaders(req, allowedOrigin);
        return new StatusCodeResult(StatusCodes.Status204NoContent);
    }

    private static void ApplyCorsHeaders(HttpRequest req, string? allowedOrigin)
    {
        if (allowedOrigin is null) return;

        var headers = req.HttpContext.Response.Headers;
        headers.AccessControlAllowOrigin  = allowedOrigin;
        headers.AccessControlAllowHeaders = "Authorization, Content-Type";
        headers.AccessControlAllowMethods = "GET, POST, OPTIONS";
        headers.AccessControlMaxAge       = "3600";
        headers.Vary                      = "Origin";
    }

    private static async Task<string?> ReadPayloadAsync(HttpRequest req)
    {
        if (HttpMethods.IsGet(req.Method))
            return req.Query.TryGetValue(PayloadQueryKey, out var queryPayload) ? queryPayload.ToString() : null;

        using var bodyReader = new StreamReader(req.Body);
        var body = await bodyReader.ReadToEndAsync();
        return string.IsNullOrWhiteSpace(body) ? null : body;
    }
}
