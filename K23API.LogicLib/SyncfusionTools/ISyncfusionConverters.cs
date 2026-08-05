namespace K23API.LogicLib.SyncfusionTools;

public interface ISyncfusionConverters
{
    byte[] DocxToPdf(byte[] docxBytes);
    byte[] XlsxToPdf(byte[] xlsxBytes);
}
