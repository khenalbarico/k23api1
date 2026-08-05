namespace K23API.LogicLib.ApiModels;

public class DocumentManagerResp1
{
    public string FileName          { get; set; } = "";
    public string ObjectKey         { get; set; } = "";
    public string UploadUrl         { get; set; } = "";
    public string DownloadUrl       { get; set; } = "";
    public long FileSizeBytes       { get; set; }
    public string SourceFormat      { get; set; } = "";
    public int ExpiresInSeconds     { get; set; }
}
