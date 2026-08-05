using K23API.Tools.Tables;

namespace K23API.Tools.Ops;

public class AppsMetadataRemoveOp1 : IConsoleOp1
{
    public string Label => "- Apps Metadata";

    public async Task RunAsync(ConsoleOpCtx1 context, CancellationToken cancellationToken)
    {
        ConsolePrompt1.Heading("Remove app metadata");

        var table = await context.OpenTableAsync(AppCatalogTables1.AppsTableName, cancellationToken);
        var apps  = new List<AppMetadataEntity1>();

        var query = table.QueryAsync<AppMetadataEntity1>(
            app => app.RowKey == AppCatalogTables1.MetadataRowKey,
            cancellationToken: cancellationToken);

        await foreach (var app in query) apps.Add(app);

        if (apps.Count == 0)
        {
            ConsolePrompt1.Warn("No app metadata exists yet.");
            return;
        }

        apps = apps.OrderBy(app => app.app_title, StringComparer.OrdinalIgnoreCase).ToList();

        var choice = ConsolePrompt1.ChooseOne(
            "Apps",
            apps.Select(app => $"{app.app_title} ({app.PartitionKey})").ToArray(),
            "Cancel");

        if (choice is null) return;

        var selected = apps[choice.Value];

        if (!ConsolePrompt1.ConfirmTyped($"Delete '{selected.app_title}' from {context.Environment}?", selected.PartitionKey))
        {
            ConsolePrompt1.Info("Cancelled.");
            return;
        }

        await table.DeleteEntityAsync(selected.PartitionKey, selected.RowKey, cancellationToken: cancellationToken);
        ConsolePrompt1.Success($"Deleted '{selected.PartitionKey}'.");
    }
}
