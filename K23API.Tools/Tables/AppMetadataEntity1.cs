using Azure;
using Azure.Data.Tables;

namespace K23API.Tools.Tables;

public class AppMetadataEntity1 : ITableEntity
{
    public string PartitionKey { get; set; } = "";
    public string RowKey       { get; set; } = AppCatalogTables1.MetadataRowKey;

    public string app_title       { get; set; } = "";
    public string app_description { get; set; } = "";
    public string app_image_url   { get; set; } = "";
    public string api_class_name  { get; set; } = "";
    public string api_method_name { get; set; } = "";
    public string categories      { get; set; } = "";

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag                 { get; set; }
}
