using K23API.LogicLib.AuthVerifier;
using K23API.LogicLib.CloudFlareTools;
using K23API.LogicLib.SyncfusionTools;
using K23API.LogicLib.TableStorageTools;

namespace K23API.LogicLib.CentralApi;

internal class ApiCfg1 : ISyncfusionCfg, IFirebaseCfg, IR2ObjectCfg, IApiGateCfg, ITableStorageCfg
{
    public string SyncfusionLicenseKey         { get; set; } = "";
    public string FirebaseProjectId            { get; set; } = "";
    public string TableStorageConnectionString { get; set; } = "";
    public string R2AccountId                  { get; set; } = "";
    public string R2AccessKeyId                { get; set; } = "";
    public string R2SecretAccessKey            { get; set; } = "";
    public string R2PrivateBucket              { get; set; } = "k23privatebucket1";
    public string R2PublicBaseUrl              { get; set; } = "";
    public int R2PresignExpirySeconds          { get; set; } = 900;
    public string[] AllowedOrigins             { get; set; } = [];

    IReadOnlyList<string> IApiGateCfg.AllowedOrigins => AllowedOrigins;
}
