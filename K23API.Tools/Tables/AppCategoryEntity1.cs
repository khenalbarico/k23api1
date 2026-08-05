using Azure;
using Azure.Data.Tables;

namespace K23API.Tools.Tables;

public class AppCategoryEntity1 : ITableEntity
{
    public string PartitionKey { get; set; } = AppCatalogTables1.CategoryPartitionKey;
    public string RowKey       { get; set; } = "";

    public string category_label { get; set; } = "";

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag                 { get; set; }
}
