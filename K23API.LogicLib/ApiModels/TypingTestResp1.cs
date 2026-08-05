namespace K23API.LogicLib.ApiModels;

public class TypingTestResp1
{
    public Dictionary<string, string> Files { get; set; } = [];

    public string LoaderUrl        { get; set; } = "";
    public string DataUrl          { get; set; } = "";
    public string FrameworkUrl     { get; set; } = "";
    public string CodeUrl          { get; set; } = "";
    public int ExpiresInSeconds    { get; set; }
}
