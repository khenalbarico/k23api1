namespace K23API.LogicLib.Apps.DocumentManager;

public static class DocumentLimits1
{
    public const int MaxFileBytes = 10 * 1024 * 1024;

    public const string DocxExtension = ".docx";
    public const string XlsxExtension = ".xlsx";

    public static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];
}
