using K23API.LogicLib.AuthVerifier;
using K23API.LogicLib.CentralApi;
using Microsoft.Extensions.DependencyInjection;

namespace K23API.LogicLib;

public static class ServiceRegistry1
{
    public static void RegisterServices(this IServiceCollection svc)
    {
        svc.LoadApiCfg();

        svc.AddMemoryCache();
        svc.AddSingleton<IApiAuthVerifier, FirebaseAdminAuthVerfier1>();
        svc.AddSingleton<ApiGate1>();
        svc.AddSingleton<ApiDispatcher1>();

        svc.AddSingleton<IApiEndpoint, ApiHealthEndpoint1>();
    }
}
