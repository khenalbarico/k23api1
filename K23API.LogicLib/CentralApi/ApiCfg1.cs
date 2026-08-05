using K23API.LogicLib.AuthVerifier;
using K23API.LogicLib.CloudFlareTools;
using K23API.LogicLib.SyncfusionTools;

namespace K23API.LogicLib.CentralApi;

internal class ApiCfg1 : ISyncfusionCfg, IFirebaseCfg, IR2ObjectCfg, IApiGateCfg
{
    public string SyncfusionLicenseKey { get; set; } = "";
    public string FirebaseProjectId    { get; set; } = "";
    public string[] AllowedOrigins     { get; set; } = [];

    IReadOnlyList<string> IApiGateCfg.AllowedOrigins => AllowedOrigins;
}
