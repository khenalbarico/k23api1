using Azure;
using Azure.Data.Tables;

namespace K23API.LogicLib.RateLimiting;

public class RateLimitEntity1 : ITableEntity
{
    public string PartitionKey { get; set; } = "";
    public string RowKey       { get; set; } = "";

    public int RequestCount              { get; set; }
    public DateTimeOffset WindowEndsAt   { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag                 { get; set; }
}
