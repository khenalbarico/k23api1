using Azure.Data.Tables;
using K23API.Tools.Tables;

namespace K23API.Tools.Ops;

public static class AppCategoryReader1
{
    public static async Task<IReadOnlyList<AppCategoryEntity1>> ReadAllAsync(
        TableClient table, CancellationToken cancellationToken)
    {
        var categories = new List<AppCategoryEntity1>();

        var query = table.QueryAsync<AppCategoryEntity1>(
            category => category.PartitionKey == AppCatalogTables1.CategoryPartitionKey,
            cancellationToken: cancellationToken);

        await foreach (var category in query) categories.Add(category);

        return categories.OrderBy(category => category.category_label, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
