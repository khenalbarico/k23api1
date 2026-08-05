using Azure.Data.Tables;

namespace K23API.Tools;

public class ConsoleOpCtx1(ToolEnvironment1 environment, string connectionString)
{
    private readonly TableServiceClient _tableService = new(connectionString);

    public ToolEnvironment1 Environment { get; } = environment;

    public bool IsProd => Environment == ToolEnvironment1.Prod;

    public async Task<TableClient> OpenTableAsync(string tableName, CancellationToken cancellationToken)
    {
        var table = _tableService.GetTableClient(tableName);
        await table.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return table;
    }
}
