using System.Text;

namespace K23API.Tools.Tables;

public static class AppCatalogTables1
{
    public const string AppsTableName       = "Apps";
    public const string CategoriesTableName = "AppCategories";

    public const string MetadataRowKey      = "metadata";
    public const string CategoryPartitionKey = "category";

    public static string ToSlug(string text)
    {
        var slug = new StringBuilder(text.Length);
        var lastWasSeparator = false;

        foreach (var character in text.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                slug.Append(character);
                lastWasSeparator = false;
                continue;
            }

            if (lastWasSeparator || slug.Length == 0) continue;

            slug.Append('-');
            lastWasSeparator = true;
        }

        return slug.ToString().Trim('-');
    }
}
