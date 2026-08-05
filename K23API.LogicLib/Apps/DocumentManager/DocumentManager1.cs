using K23API.LogicLib.ApiModels;
using K23API.LogicLib.CentralApi;
using K23API.LogicLib.CloudFlareTools;
using K23API.LogicLib.SyncfusionTools;

namespace K23API.LogicLib.Apps.DocumentManager;

public class DocumentManager1(IR2Objects r2Objects, ISyncfusionConverters syncfusionConverters) : IDocumentManager
{
    private const string AppSlug = "document-manager";
    private const string PdfContentType = "application/pdf";

    public Task<DocumentManagerResp1> CreateUploadUrl(DocumentManagerReq1 request, ApiCall1 call)
    {
        var sourceFormat = SourceFormatOf(request.FileName);
        var objectKey    = R2ObjectKey1.ForPrivateUpload(AppSlug, call.CallerUid, Guid.NewGuid().ToString("n"), request.FileName);

        return Task.FromResult(new DocumentManagerResp1
        {
            FileName          = request.FileName,
            ObjectKey         = objectKey,
            UploadUrl         = r2Objects.CreateUploadUrl(objectKey),
            SourceFormat      = sourceFormat,
            ExpiresInSeconds  = r2Objects.PresignExpirySeconds
        });
    }

    public async Task<DocumentManagerResp1> ConvertToPdf(
        DocumentManagerReq1 request, ApiCall1 call, CancellationToken cancellationToken)
    {
        var sourceObjectKey = EnsureCallerOwnsUpload(request.ObjectKey, call.CallerUid);
        var sourceFormat    = SourceFormatOf(sourceObjectKey);

        await EnsureUploadWithinLimitAsync(sourceObjectKey, cancellationToken);

        var sourceBytes = await r2Objects.ReadAsync(sourceObjectKey, cancellationToken);
        EnsureLooksLikeOfficeFile(sourceBytes);

        var pdfBytes = sourceFormat switch
        {
            DocumentLimits1.DocxExtension => syncfusionConverters.DocxToPdf(sourceBytes),
            DocumentLimits1.XlsxExtension => syncfusionConverters.XlsxToPdf(sourceBytes),
            _                             => throw UnsupportedFormat()
        };

        var pdfFileName  = $"{Path.GetFileNameWithoutExtension(sourceObjectKey)}.pdf";
        var pdfObjectKey = R2ObjectKey1.ForPrivateResult(sourceObjectKey, pdfFileName);

        await r2Objects.WriteAsync(pdfObjectKey, pdfBytes, PdfContentType, cancellationToken);

        return new DocumentManagerResp1
        {
            FileName         = pdfFileName,
            ObjectKey        = pdfObjectKey,
            DownloadUrl      = r2Objects.CreateDownloadUrl(pdfObjectKey),
            FileSizeBytes    = pdfBytes.Length,
            SourceFormat     = sourceFormat,
            ExpiresInSeconds = r2Objects.PresignExpirySeconds
        };
    }

    private static string EnsureCallerOwnsUpload(string objectKey, string callerUid)
    {
        if (string.IsNullOrWhiteSpace(objectKey)) throw ApiEx1.BadRequest("An uploaded file is required.");

        if (!R2ObjectKey1.IsOwnedBy(objectKey, AppSlug, callerUid) || !R2ObjectKey1.IsUploadKey(objectKey))
            throw ApiEx1.Forbidden();

        return objectKey;
    }

    private async Task EnsureUploadWithinLimitAsync(string objectKey, CancellationToken cancellationToken)
    {
        var uploadedBytes = await r2Objects.GetSizeAsync(objectKey, cancellationToken)
                            ?? throw ApiEx1.BadRequest("The uploaded file was not found. Please upload it again.");

        if (uploadedBytes == 0) throw ApiEx1.BadRequest("The uploaded file is empty.");

        if (uploadedBytes > DocumentLimits1.MaxFileBytes)
            throw ApiEx1.BadRequest($"Files must be {DocumentLimits1.MaxFileBytes / (1024 * 1024)} MB or smaller.");
    }

    private static string SourceFormatOf(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw ApiEx1.BadRequest("A file name is required.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension is DocumentLimits1.DocxExtension or DocumentLimits1.XlsxExtension
            ? extension
            : throw UnsupportedFormat();
    }

    private static void EnsureLooksLikeOfficeFile(byte[] uploadBytes)
    {
        if (!uploadBytes.Take(DocumentLimits1.ZipSignature.Length).SequenceEqual(DocumentLimits1.ZipSignature))
            throw ApiEx1.BadRequest("The uploaded file does not match its file extension.");
    }

    private static ApiEx1 UnsupportedFormat() =>
        ApiEx1.BadRequest("Only .docx and .xlsx files can be converted to PDF.");
}
