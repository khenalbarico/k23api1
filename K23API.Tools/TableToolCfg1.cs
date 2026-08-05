using System.Text.Json;

namespace K23API.Tools;

public class TableToolCfg1
{
    public const string SettingsFileName = "tabletool.settings.json";

    public string DevConnectionString  { get; set; } = "";
    public string ProdConnectionString { get; set; } = "";

    public string ConnectionStringFor(ToolEnvironment1 environment) => environment switch
    {
        ToolEnvironment1.Prod => ProdConnectionString,
        _                     => DevConnectionString
    };

    public static TableToolCfg1? Load()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);

        if (!File.Exists(settingsPath))
        {
            ConsolePrompt1.Error($"'{SettingsFileName}' was not found next to the executable.");
            ConsolePrompt1.Hint($"Copy 'tabletool.settings.example.json' to '{SettingsFileName}' and fill in your connection strings.");
            return null;
        }

        try
        {
            var settings = JsonSerializer.Deserialize<TableToolCfg1>(
                File.ReadAllText(settingsPath),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (settings is null) ConsolePrompt1.Error($"'{SettingsFileName}' is empty.");
            return settings;
        }
        catch (JsonException exception)
        {
            ConsolePrompt1.Error($"'{SettingsFileName}' is not valid JSON: {exception.Message}");
            return null;
        }
    }
}
