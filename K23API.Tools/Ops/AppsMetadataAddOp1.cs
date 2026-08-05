using Azure;
using Azure.Data.Tables;
using K23API.Tools.Tables;

namespace K23API.Tools.Ops;

public class AppsMetadataAddOp1 : IConsoleOp1
{
    public string Label => "+ Apps Metadata";

    public async Task RunAsync(ConsoleOpCtx1 context, CancellationToken cancellationToken)
    {
        ConsolePrompt1.Heading("Add app metadata");

        var title = ConsolePrompt1.ReadRequired("Title");
        if (title is null) return;

        var slug = AppCatalogTables1.ToSlug(title);

        if (slug.Length == 0)
        {
            ConsolePrompt1.Error("That title has no usable characters for a key.");
            return;
        }

        ConsolePrompt1.Hint($"Slug: {slug}");

        var description = ConsolePrompt1.ReadRequired("Description");
        if (description is null) return;

        var imageUrl = ConsolePrompt1.ReadRequired("ImageUrl");
        if (imageUrl is null) return;

        var apiClassName = ConsolePrompt1.ReadRequired("ApiClassName");
        if (apiClassName is null) return;

        var apiMethodName = ConsolePrompt1.ReadRequired("ApiMethodName");
        if (apiMethodName is null) return;

        var categories = await PickCategoriesAsync(context, cancellationToken);
        if (categories is null) return;

        var table = await context.OpenTableAsync(AppCatalogTables1.AppsTableName, cancellationToken);

        var metadata = new AppMetadataEntity1
        {
            PartitionKey    = slug,
            app_title       = title,
            app_description = description,
            app_image_url   = imageUrl,
            api_class_name  = apiClassName,
            api_method_name = apiMethodName,
            categories      = string.Join(',', categories)
        };

        try
        {
            await table.AddEntityAsync(metadata, cancellationToken);
            ConsolePrompt1.Success($"Added '{title}' ({slug}) to {AppCatalogTables1.AppsTableName}.");
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            ConsolePrompt1.Warn($"'{slug}' already exists.");

            if (!ConsolePrompt1.Confirm("Overwrite it?")) return;

            await table.UpsertEntityAsync(metadata, TableUpdateMode.Replace, cancellationToken);
            ConsolePrompt1.Success($"Replaced '{slug}'.");
        }
    }

    private static async Task<IReadOnlyList<string>?> PickCategoriesAsync(
        ConsoleOpCtx1 context, CancellationToken cancellationToken)
    {
        var categoryTable = await context.OpenTableAsync(AppCatalogTables1.CategoriesTableName, cancellationToken);
        var categories    = await AppCategoryReader1.ReadAllAsync(categoryTable, cancellationToken);

        if (categories.Count == 0)
        {
            ConsolePrompt1.Warn("No categories exist yet, so this app will be saved without any.");
            ConsolePrompt1.Hint("Add some with '+ Apps Categories' first if you want them.");
            return [];
        }

        var chosen = ConsolePrompt1.ChooseMany(
            "Categories",
            categories.Select(category => $"{category.category_label} ({category.RowKey})").ToArray());

        return chosen?.Select(index => categories[index].RowKey).ToArray();
    }
}
