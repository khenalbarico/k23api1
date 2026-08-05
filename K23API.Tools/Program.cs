using K23API.Tools;
using K23API.Tools.Ops;

IConsoleOp1[] operations =
[
    new AppsMetadataAddOp1(),
    new AppsMetadataRemoveOp1(),
    new AppsCategoryAddOp1(),
    new AppsCategoryRemoveOp1()
];

ConsolePrompt1.Heading("K23 Table Storage Tool");

var settings = TableToolCfg1.Load();
if (settings is null) return 1;

var environment = SelectEnvironment(settings);
if (environment is null) return 0;

var connectionString = settings.ConnectionStringFor(environment.Value);

if (string.IsNullOrWhiteSpace(connectionString))
{
    ConsolePrompt1.Error($"No connection string is set for {environment} in {TableToolCfg1.SettingsFileName}.");
    return 1;
}

var context = new ConsoleOpCtx1(environment.Value, connectionString);

ConsolePrompt1.Info($"Connected to {environment}.");

while (true)
{
    var choice = ConsolePrompt1.ChooseOne("What should it do?", operations.Select(op => op.Label).ToArray(), "Exit");
    if (choice is null) break;

    try
    {
        await operations[choice.Value].RunAsync(context, CancellationToken.None);
    }
    catch (Exception exception)
    {
        ConsolePrompt1.Error(exception.Message);
    }
}

ConsolePrompt1.Info("Done.");
return 0;

static ToolEnvironment1? SelectEnvironment(TableToolCfg1 settings)
{
    var choice = ConsolePrompt1.ChooseOne("Environment", ["dev", "prod"], "Exit");

    if (choice is null) return null;
    if (choice == 0) return ToolEnvironment1.Dev;

    ConsolePrompt1.Warn("PROD writes to live data used by real users.");

    return ConsolePrompt1.ConfirmTyped("Continue against PROD?", "PROD")
        ? ToolEnvironment1.Prod
        : null;
}
