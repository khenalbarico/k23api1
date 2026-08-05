using System.Reflection;
using K23API.LogicLib.RateLimiting;

namespace K23API.LogicLib.CentralApi;

public class ApiMethodDescriptor1
{
    public required Type ApiInterface       { get; init; }
    public required MethodInfo Method       { get; init; }
    public required bool RequiresAuth       { get; init; }
    public required ApiRateLimit1 RateLimit { get; init; }

    public string ApiClassName  => ApiInterface.Name;
    public string ApiMethodName => Method.Name;
}
