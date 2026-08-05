namespace K23API.LogicLib.CloudFlareTools;

public interface IR2Objects
{
    int PresignExpirySeconds { get; }

    string CreateUploadUrl(string objectKey);
    string CreateDownloadUrl(string objectKey);

    Task<long?> GetSizeAsync(string objectKey, CancellationToken cancellationToken);
    Task<byte[]> ReadAsync(string objectKey, CancellationToken cancellationToken);
    Task WriteAsync(string objectKey, byte[] content, string contentType, CancellationToken cancellationToken);
}
