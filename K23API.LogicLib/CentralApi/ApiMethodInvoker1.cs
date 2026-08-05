using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace K23API.LogicLib.CentralApi;

public class ApiMethodInvoker1(IServiceProvider services)
{
    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task<object?> InvokeAsync(
        ApiMethodDescriptor1 descriptor, ApiCall1 call, CancellationToken cancellationToken)
    {
        var apiClass = services.GetService(descriptor.ApiInterface)
                       ?? throw new InvalidOperationException(
                           $"'{descriptor.ApiClassName}' is registered for dispatch but not resolvable from DI.");

        var arguments = BindArguments(descriptor, call, cancellationToken);

        try
        {
            return await UnwrapResultAsync(descriptor.Method.Invoke(apiClass, arguments), descriptor.Method);
        }
        catch (TargetInvocationException invocationException) when (invocationException.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
            throw;
        }
    }

    private static object?[] BindArguments(
        ApiMethodDescriptor1 descriptor, ApiCall1 call, CancellationToken cancellationToken)
    {
        var parameters = descriptor.Method.GetParameters();
        var arguments  = new object?[parameters.Length];

        for (var index = 0; index < parameters.Length; index++)
        {
            var parameterType = parameters[index].ParameterType;

            arguments[index] = parameterType == typeof(CancellationToken) ? cancellationToken
                : parameterType == typeof(ApiCall1)                       ? call
                : DeserializePayload(descriptor, call, parameterType);
        }

        return arguments;
    }

    private static object DeserializePayload(ApiMethodDescriptor1 descriptor, ApiCall1 call, Type parameterType)
    {
        if (string.IsNullOrWhiteSpace(call.PayloadJson))
            throw ApiEx1.BadRequest($"'{descriptor.ApiClassName}.{descriptor.ApiMethodName}' requires a payload but none was supplied.");

        try
        {
            return JsonSerializer.Deserialize(call.PayloadJson, parameterType, PayloadOptions)
                   ?? throw ApiEx1.BadRequest($"'{descriptor.ApiClassName}.{descriptor.ApiMethodName}' received an empty payload.");
        }
        catch (JsonException)
        {
            throw ApiEx1.BadRequest($"'{descriptor.ApiClassName}.{descriptor.ApiMethodName}' received a payload that is not valid JSON.");
        }
    }

    private static async Task<object?> UnwrapResultAsync(object? invocationResult, MethodInfo method)
    {
        if (invocationResult is not Task task) return invocationResult;

        await task;

        return method.ReturnType.IsGenericType
            ? method.ReturnType.GetProperty(nameof(Task<object>.Result))!.GetValue(task)
            : null;
    }
}
