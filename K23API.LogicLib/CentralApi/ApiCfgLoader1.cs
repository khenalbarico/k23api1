using System.Globalization;
using System.Reflection;
using K23API.LogicLib.AuthVerifier;
using K23API.LogicLib.CloudFlareTools;
using K23API.LogicLib.SyncfusionTools;
using Microsoft.Extensions.DependencyInjection;

namespace K23API.LogicLib.CentralApi;

public static class ApiCfgLoader1
{
    public static void LoadApiCfg(this IServiceCollection svc)
    {
        var apiCfg = ReadFromEnvironment();

        svc.AddSingleton<ISyncfusionCfg>(apiCfg);
        svc.AddSingleton<IFirebaseCfg>(apiCfg);
        svc.AddSingleton<IR2ObjectCfg>(apiCfg);
        svc.AddSingleton<IApiGateCfg>(apiCfg);
    }

    private static ApiCfg1 ReadFromEnvironment()
    {
        var apiCfg = new ApiCfg1();

        foreach (var setting in SettableSettings())
        {
            var rawValue = Environment.GetEnvironmentVariable(setting.Name);
            if (string.IsNullOrWhiteSpace(rawValue)) continue;

            setting.SetValue(apiCfg, ConvertToSettingType(setting, rawValue.Trim()));
        }

        return apiCfg;
    }

    private static PropertyInfo[] SettableSettings() =>
        typeof(ApiCfg1)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(setting => setting.CanWrite)
            .ToArray();

    private static object ConvertToSettingType(PropertyInfo setting, string rawValue)
    {
        var settingType = Nullable.GetUnderlyingType(setting.PropertyType) ?? setting.PropertyType;

        if (settingType == typeof(string)) return rawValue;

        if (settingType == typeof(string[]))
            return rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        try
        {
            return settingType.IsEnum
                ? Enum.Parse(settingType, rawValue, ignoreCase: true)
                : Convert.ChangeType(rawValue, settingType, CultureInfo.InvariantCulture);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"App setting '{setting.Name}' is set to '{rawValue}', which is not a valid {settingType.Name}.",
                exception);
        }
    }
}
