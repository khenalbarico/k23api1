using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace K23API.LogicLib.CloudFlareTools;

public class R2Objects1 : IR2Objects, IDisposable
{
    private readonly IAmazonS3 _r2;
    private readonly string _privateBucket;

    public R2Objects1(IR2ObjectCfg r2Cfg)
    {
        RequireSetting(r2Cfg.R2AccountId, nameof(r2Cfg.R2AccountId));
        RequireSetting(r2Cfg.R2AccessKeyId, nameof(r2Cfg.R2AccessKeyId));
        RequireSetting(r2Cfg.R2SecretAccessKey, nameof(r2Cfg.R2SecretAccessKey));
        RequireSetting(r2Cfg.R2PrivateBucket, nameof(r2Cfg.R2PrivateBucket));

        _privateBucket        = r2Cfg.R2PrivateBucket;
        PresignExpirySeconds  = r2Cfg.R2PresignExpirySeconds > 0 ? r2Cfg.R2PresignExpirySeconds : 900;

        _r2 = new AmazonS3Client(
            new BasicAWSCredentials(r2Cfg.R2AccessKeyId, r2Cfg.R2SecretAccessKey),
            new AmazonS3Config
            {
                ServiceURL                 = $"https://{r2Cfg.R2AccountId}.r2.cloudflarestorage.com",
                AuthenticationRegion       = "auto",
                ForcePathStyle             = true,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
            });
    }

    public int PresignExpirySeconds { get; }

    public string CreateUploadUrl(string objectKey) => Presign(objectKey, HttpVerb.PUT);

    public string CreateDownloadUrl(string objectKey) => Presign(objectKey, HttpVerb.GET);

    public async Task<long?> GetSizeAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await _r2.GetObjectMetadataAsync(_privateBucket, objectKey, cancellationToken);
            return metadata.ContentLength;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<byte[]> ReadAsync(string objectKey, CancellationToken cancellationToken)
    {
        using var storedObject = await _r2.GetObjectAsync(_privateBucket, objectKey, cancellationToken);
        using var content = new MemoryStream();

        await storedObject.ResponseStream.CopyToAsync(content, cancellationToken);
        return content.ToArray();
    }

    public async Task WriteAsync(string objectKey, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var contentStream = new MemoryStream(content);

        await _r2.PutObjectAsync(new PutObjectRequest
        {
            BucketName  = _privateBucket,
            Key         = objectKey,
            InputStream = contentStream,
            ContentType = contentType
        }, cancellationToken);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _r2.Dispose();
    }

    private string Presign(string objectKey, HttpVerb verb) =>
        _r2.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _privateBucket,
            Key        = objectKey,
            Verb       = verb,
            Expires    = DateTime.UtcNow.AddSeconds(PresignExpirySeconds)
        });

    private static void RequireSetting(string value, string settingName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"App setting '{settingName}' is not set, so R2 object storage cannot be used.");
    }
}
