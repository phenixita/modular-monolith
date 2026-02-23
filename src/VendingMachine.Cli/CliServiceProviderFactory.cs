using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Orders;
using VendingMachine.Persistence;

internal static class CliServiceProviderFactory
{
    public static ServiceProvider Build(CliConfiguration config)
    {
        var services = new ServiceCollection();

        services.AddVendingMachineInventoryModule();
        services.AddCashRegisterModule();
        services.AddOrdersModule();
        services.AddPostgresPersistence(config.Postgres.ConnectionString);

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddSingleton<IInventoryRepository>(sp =>
            new PostgresInventoryRepository(
                config.Postgres.ConnectionString,
                sp.GetRequiredService<IPostgresTransactionAccessor>()));
        services.AddSingleton<ICashStorage>(sp =>
            new PostgresCashStorage(
                config.Postgres.ConnectionString,
                sp.GetRequiredService<IPostgresTransactionAccessor>()));
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IOrderService, OrderService>();

        return services.BuildServiceProvider();
    }
}
