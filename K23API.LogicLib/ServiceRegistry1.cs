using K23API.LogicLib.Apps.DocumentManager;
using K23API.LogicLib.Apps.TypingTest;
using K23API.LogicLib.AuthVerifier;
using K23API.LogicLib.CentralApi;
using K23API.LogicLib.CloudFlareTools;
using K23API.LogicLib.RateLimiting;
using K23API.LogicLib.SyncfusionTools;
using Microsoft.Extensions.DependencyInjection;

namespace K23API.LogicLib;

public static class ServiceRegistry1
{
    public static void RegisterServices(this IServiceCollection svc)
    {
        svc.LoadApiCfg();

        svc.AddMemoryCache();
        svc.AddSingleton<IApiAuthVerifier, FirebaseAdminAuthVerfier1>();
        svc.AddSingleton<IApiRateLimiter, TableStorageRateLimiter1>();
        svc.AddSingleton<ApiGate1>();
        svc.AddSingleton<ApiClassRegistry1>();
        svc.AddSingleton<ApiMethodInvoker1>();
        svc.AddSingleton<ApiDispatcher1>();

        svc.AddSingleton<ISyncfusionConverters, SyncfusionConverters1>();
        svc.AddSingleton<IR2Objects, R2Objects1>();
        svc.AddSingleton<R2PublicUrl1>();

        svc.AddDispatchableApi<IApiHealth, ApiHealth1>();
        svc.AddDispatchableApi<IDocumentManager, DocumentManager1>();
        svc.AddDispatchableApi<ITypingTest, TypingTest1>();
    }

    private static void AddDispatchableApi<TApiInterface, TApiClass>(this IServiceCollection svc)
        where TApiInterface : class
        where TApiClass : class, TApiInterface
    {
        svc.AddSingleton<TApiInterface, TApiClass>();
        svc.AddSingleton(new ApiClassRegistration1(typeof(TApiInterface)));
    }
}
