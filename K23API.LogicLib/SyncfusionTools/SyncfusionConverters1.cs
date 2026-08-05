using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Licensing;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

namespace K23API.LogicLib.SyncfusionTools;

public class SyncfusionConverters1 : ISyncfusionConverters
{
    private static int _licenseRegistered;

    public SyncfusionConverters1(ISyncfusionCfg syncfusionCfg)
    {
        if (string.IsNullOrWhiteSpace(syncfusionCfg.SyncfusionLicenseKey)) return;
        if (Interlocked.Exchange(ref _licenseRegistered, 1) == 1) return;

        SyncfusionLicenseProvider.RegisterLicense(syncfusionCfg.SyncfusionLicenseKey);
    }

    public byte[] DocxToPdf(byte[] docxBytes)
    {
        using var docxStream = new MemoryStream(docxBytes);
        using var wordDocument = new WordDocument(docxStream, FormatType.Docx);
        using var renderer = new DocIORenderer();
        using var pdfDocument = renderer.ConvertToPDF(wordDocument);

        return SavePdf(pdfDocument.Save);
    }

    public byte[] XlsxToPdf(byte[] xlsxBytes)
    {
        using var excelEngine = new ExcelEngine();
        using var xlsxStream = new MemoryStream(xlsxBytes);

        var workbook = excelEngine.Excel.Workbooks.Open(xlsxStream);
        var renderer = new XlsIORenderer();
        using var pdfDocument = renderer.ConvertToPDF(workbook);

        return SavePdf(pdfDocument.Save);
    }

    private static byte[] SavePdf(Action<Stream> savePdf)
    {
        using var pdfStream = new MemoryStream();
        savePdf(pdfStream);
        return pdfStream.ToArray();
    }
}
