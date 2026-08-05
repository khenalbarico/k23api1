namespace K23API.LogicLib.CentralApi;

public interface IApiGateCfg
{
    IReadOnlyList<string> AllowedOrigins { get; }
}
