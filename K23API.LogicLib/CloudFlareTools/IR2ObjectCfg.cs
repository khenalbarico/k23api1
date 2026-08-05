namespace K23API.LogicLib.CloudFlareTools;

public interface IR2ObjectCfg
{
    string R2AccountId          { get; }
    string R2AccessKeyId        { get; }
    string R2SecretAccessKey    { get; }
    string R2PrivateBucket      { get; }
    int R2PresignExpirySeconds  { get; }
}
