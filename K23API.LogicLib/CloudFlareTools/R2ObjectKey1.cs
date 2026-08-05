using System.Text;

namespace K23API.LogicLib.CloudFlareTools;

public static class R2ObjectKey1
{
    public static string PrivatePrefixFor(string appSlug, string callerUid) =>
        $"{appSlug}/private/{Segment(callerUid)}/";

    public static string ForPrivateUpload(string appSlug, string callerUid, string uploadId, string fileName) =>
        $"{PrivatePrefixFor(appSlug, callerUid)}{Segment(uploadId)}/source/{Segment(fileName)}";

    public static string ForPrivateResult(string sourceObjectKey, string fileName) =>
        $"{sourceObjectKey[..sourceObjectKey.LastIndexOf("/source/", StringComparison.Ordinal)]}/result/{Segment(fileName)}";

    public static bool IsOwnedBy(string objectKey, string appSlug, string callerUid) =>
        !string.IsNullOrWhiteSpace(callerUid) &&
        objectKey.StartsWith(PrivatePrefixFor(appSlug, callerUid), StringComparison.Ordinal) &&
        !objectKey.Contains("..", StringComparison.Ordinal);

    public static bool IsUploadKey(string objectKey) =>
        objectKey.Contains("/source/", StringComparison.Ordinal);

    private static string Segment(string rawSegment)
    {
        var segment = new StringBuilder(rawSegment.Length);

        foreach (var character in rawSegment)
            segment.Append(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '-');

        return segment.ToString();
    }
}
