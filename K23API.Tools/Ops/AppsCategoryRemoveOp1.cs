using K23API.Tools.Tables;

namespace K23API.Tools.Ops;

public class AppsCategoryRemoveOp1 : IConsoleOp1
{
    public string Label => "- Apps Categories";

    public async Task RunAsync(ConsoleOpCtx1 context, CancellationToken cancellationToken)
    {
        ConsolePrompt1.Heading("Remove app category");

        var table      = await context.OpenTableAsync(AppCatalogTables1.CategoriesTableName, cancellationToken);
        var categories = await AppCategoryReader1.ReadAllAsync(table, cancellationToken);

        if (categories.Count == 0)
        {
            ConsolePrompt1.Warn("No categories exist yet.");
            return;
        }

        var choice = ConsolePrompt1.ChooseOne(
            "Categories",
            categories.Select(category => $"{category.category_label} ({category.RowKey})").ToArray(),
            "Cancel");

        if (choice is null) return;

        var selected = categories[choice.Value];

        if (!ConsolePrompt1.Confirm($"Delete category '{selected.category_label}'?"))
        {
            ConsolePrompt1.Info("Cancelled.");
            return;
        }

        await table.DeleteEntityAsync(selected.PartitionKey, selected.RowKey, cancellationToken: cancellationToken);
        ConsolePrompt1.Success($"Deleted category '{selected.RowKey}'.");
        ConsolePrompt1.Hint("Apps already tagged with this category keep the tag until you edit them.");
    }
}
