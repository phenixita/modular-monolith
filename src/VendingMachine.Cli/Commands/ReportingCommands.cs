using System.CommandLine;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Reporting.Abstractions;

internal static class ReportingCommands
{
    public static Command Build(CliOptions options)
    {
        var reportingCommand = new Command("reporting", "Reporting and analytics.");

        var dashboardCommand = new Command("dashboard", "Show dashboard statistics. Example: vm reporting dashboard");

        dashboardCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var reportingService = scope.ServiceProvider.GetRequiredService<IReportingService>();

                var stats = await reportingService.GetDashboardStatsAsync();

                CliOutputWriter.Write(format,
                    new
                    {
                        totaleOrdiniEuro = stats.TotalRevenue,
                        totaleNumeroOrdini = stats.OrderCount,
                        mediaEuroOrdini = stats.AverageOrderValue
                    },
                    $"Total revenue: {CliParsing.FormatMoney(stats.TotalRevenue)}{Environment.NewLine}Total orders: {stats.OrderCount.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}Average order: {CliParsing.FormatMoney(stats.AverageOrderValue)}");
            }, context);
        });

        reportingCommand.AddCommand(dashboardCommand);
        return reportingCommand;
    }
}
