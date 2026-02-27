using System.CommandLine;

internal sealed class CliApp
{
    private readonly CliOptions _options = CliOptions.CreateDefault();

    public Task<int> RunAsync(string[] args)
    {
        var root = new RootCommand(
            "Vending machine CLI for catalog, stock, cash, and orders. Use --help on any command for examples.");
        root.AddGlobalOption(_options.FormatOption);
        root.AddGlobalOption(_options.ConfigOption);

        root.AddCommand(InventoryCommands.Build(_options));
        root.AddCommand(CashCommands.Build(_options));
        root.AddCommand(OrderCommands.Build(_options));
        root.AddCommand(ReportingCommands.Build(_options));

        return root.InvokeAsync(args);
    }
}
