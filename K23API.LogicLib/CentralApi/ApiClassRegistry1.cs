using System.Reflection;
using K23API.LogicLib.RateLimiting;

namespace K23API.LogicLib.CentralApi;

public class ApiClassRegistry1
{
    private readonly IReadOnlyDictionary<string, ApiMethodDescriptor1> _methodsByName;

    public ApiClassRegistry1(IEnumerable<ApiClassRegistration1> registrations)
    {
        var methodsByName = new Dictionary<string, ApiMethodDescriptor1>(StringComparer.OrdinalIgnoreCase);

        foreach (var registration in registrations)
            foreach (var descriptor in DescribeDispatchableMethods(registration.ApiInterface))
                methodsByName[MethodKey(descriptor.ApiClassName, descriptor.ApiMethodName)] = descriptor;

        _methodsByName = methodsByName;
    }

    public IReadOnlyCollection<ApiMethodDescriptor1> DispatchableMethods => _methodsByName.Values.ToArray();

    public ApiMethodDescriptor1? Find(string apiClassName, string apiMethodName) =>
        _methodsByName.GetValueOrDefault(MethodKey(apiClassName, apiMethodName));

    private static IEnumerable<ApiMethodDescriptor1> DescribeDispatchableMethods(Type apiInterface)
    {
        foreach (var method in apiInterface.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var apiMethod = method.GetCustomAttribute<ApiMethod1>();
            if (apiMethod is null) continue;

            EnsureBindableSignature(apiInterface, method);

            yield return new ApiMethodDescriptor1
            {
                ApiInterface = apiInterface,
                Method       = method,
                RequiresAuth = apiMethod.RequiresAuth,
                RateLimit    = ApiRateLimitTiers1.Resolve(apiMethod.RateLimit)
            };
        }
    }

    private static void EnsureBindableSignature(Type apiInterface, MethodInfo method)
    {
        var payloadParameters = method.GetParameters().Count(parameter =>
            parameter.ParameterType != typeof(CancellationToken) && parameter.ParameterType != typeof(ApiCall1));

        if (payloadParameters > 1)
            throw new InvalidOperationException(
                $"'{apiInterface.Name}.{method.Name}' takes {payloadParameters} payload parameters. " +
                "A dispatchable method may take at most one payload parameter, plus optional ApiCall1 and CancellationToken.");

        if (method.IsGenericMethod)
            throw new InvalidOperationException($"'{apiInterface.Name}.{method.Name}' is generic and cannot be dispatched.");
    }

    private static string MethodKey(string apiClassName, string apiMethodName) => $"{apiClassName}/{apiMethodName}";
}
