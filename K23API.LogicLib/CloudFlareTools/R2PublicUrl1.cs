namespace K23API.LogicLib.CloudFlareTools;

public class R2PublicUrl1(IR2ObjectCfg r2Cfg)
{
    private readonly string _publicBaseUrl = r2Cfg.R2PublicBaseUrl.TrimEnd('/');

    public string For(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(_publicBaseUrl))
            throw new InvalidOperationException("App setting 'R2PublicBaseUrl' is not set, so public asset URLs cannot be built.");

        return $"{_publicBaseUrl}/{objectKey.TrimStart('/')}";
    }

    public string ForAppIcon(string appSlug) => For($"apps/{appSlug}/icon.png");
}
