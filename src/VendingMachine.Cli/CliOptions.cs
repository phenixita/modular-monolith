using System.CommandLine;

internal sealed class CliOptions
{
    private CliOptions(Option<string> formatOption, Option<FileInfo> configOption, string defaultConfigPath)
    {
        FormatOption = formatOption;
        ConfigOption = configOption;
        DefaultConfigPath = defaultConfigPath;
    }

    public Option<string> FormatOption { get; }

    public Option<FileInfo> ConfigOption { get; }

    public string DefaultConfigPath { get; }

    public static CliOptions CreateDefault()
    {
        var defaultConfigPath = GetDefaultConfigPath();

        var formatOption = new Option<string>("--format", () => "text",
            "Output format: text (default) or json.");
        formatOption.FromAmong("text", "json");

        var configOption = new Option<FileInfo>("--config", () => new FileInfo(defaultConfigPath),
            $"Path to CLI config file. Default: {defaultConfigPath}");

        return new CliOptions(formatOption, configOption, defaultConfigPath);
    }

    private static string GetDefaultConfigPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(basePath, "VendingMachine", "cli.json");
    }
}
