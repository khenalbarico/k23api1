using K23API.LogicLib.ApiModels;
using K23API.LogicLib.CentralApi;
using K23API.LogicLib.CloudFlareTools;

namespace K23API.LogicLib.Apps.TypingTest;

public class TypingTest1(IR2Objects r2Objects) : ITypingTest
{
    private const string WebGlPrefix = "apps/typing-test/webgl/";

    private static readonly string[] CompressionSuffixes = [".br", ".gz", ".unityweb"];

    public async Task<TypingTestResp1> LoadWebGL(CancellationToken cancellationToken)
    {
        var keys = await r2Objects.ListKeysAsync(WebGlPrefix, cancellationToken);

        if (keys.Count == 0)
            throw ApiEx1.NotFound("The Typing Test build has not been published yet.");

        var files = keys.ToDictionary(
            key => key[WebGlPrefix.Length..],
            r2Objects.CreateDownloadUrl,
            StringComparer.OrdinalIgnoreCase);

        return new TypingTestResp1
        {
            Files            = files,
            LoaderUrl        = UnityFileUrl(files, ".loader.js"),
            DataUrl          = UnityFileUrl(files, ".data"),
            FrameworkUrl     = UnityFileUrl(files, ".framework.js"),
            CodeUrl          = UnityFileUrl(files, ".wasm"),
            ExpiresInSeconds = r2Objects.PresignExpirySeconds
        };
    }

    private static string UnityFileUrl(IReadOnlyDictionary<string, string> files, string unitySuffix)
    {
        foreach (var (relativePath, presignedUrl) in files)
            if (WithoutCompressionSuffix(relativePath).EndsWith(unitySuffix, StringComparison.OrdinalIgnoreCase))
                return presignedUrl;

        return "";
    }

    private static string WithoutCompressionSuffix(string relativePath)
    {
        foreach (var compression in CompressionSuffixes)
            if (relativePath.EndsWith(compression, StringComparison.OrdinalIgnoreCase))
                return relativePath[..^compression.Length];

        return relativePath;
    }
}
