using System.CommandLine.Invocation;

internal static class CliCommandExecution
{
    public static string GetFormat(InvocationContext context, CliOptions options) =>
        context.ParseResult.GetValueForOption(options.FormatOption) ?? "text";

    public static FileInfo GetConfigFile(InvocationContext context, CliOptions options) =>
        context.ParseResult.GetValueForOption(options.ConfigOption)
        ?? new FileInfo(options.DefaultConfigPath);

    public static async Task<int> ExecuteAsync(Func<Task> action, InvocationContext context)
    {
        try
        {
            await action();
            return 0;
        }
        catch (Exception ex)
        {
            context.Console.Error.Write($"Error: {ex.Message}{Environment.NewLine}");
            return 1;
        }
    }
}
