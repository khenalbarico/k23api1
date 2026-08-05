using Azure;
using K23API.Tools.Tables;

namespace K23API.Tools.Ops;

public class AppsCategoryAddOp1 : IConsoleOp1
{
    public string Label => "+ Apps Categories";

    public async Task RunAsync(ConsoleOpCtx1 context, CancellationToken cancellationToken)
    {
        ConsolePrompt1.Heading("Add app category");

        var label = ConsolePrompt1.ReadRequired("Category label");
        if (label is null) return;

        var slug = AppCatalogTables1.ToSlug(label);

        if (slug.Length == 0)
        {
            ConsolePrompt1.Error("That label has no usable characters for a key.");
            return;
        }

        var table = await context.OpenTableAsync(AppCatalogTables1.CategoriesTableName, cancellationToken);

        var category = new AppCategoryEntity1
        {
            RowKey         = slug,
            category_label = label
        };

        try
        {
            await table.AddEntityAsync(category, cancellationToken);
            ConsolePrompt1.Success($"Added category '{label}' ({slug}).");
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            ConsolePrompt1.Warn($"Category '{slug}' already exists.");
        }
    }
}
