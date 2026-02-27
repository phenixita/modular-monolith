using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Orders;
using VendingMachine.Persistence;
using VendingMachine.Reporting;
using VendingMachine.Reporting.Abstractions;

internal static class CliServiceProviderFactory
{
    public static ServiceProvider Build(CliConfiguration config)
    {
        var services = new ServiceCollection();

        services.AddInventoryModule();
        services.AddCashRegisterModule();
        services.AddOrdersModule();
        services.AddReportingModule();
        services.AddPostgresPersistence(config.Postgres.ConnectionString);

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddSingleton<IInventoryRepository>(sp =>
            new PostgresInventoryRepository(
                config.Postgres.ConnectionString,
                sp.GetRequiredService<ITransactionContext>()));
        services.AddSingleton<ICashRepository>(sp =>
            new PostgresCashRepository(
                config.Postgres.ConnectionString,
                sp.GetRequiredService<ITransactionContext>()));
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReportingService, ReportingService>();

        return services.BuildServiceProvider();
    }
}
