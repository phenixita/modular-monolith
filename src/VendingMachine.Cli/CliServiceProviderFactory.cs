using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;

internal static class CliServiceProviderFactory
{
    public static ServiceProvider Build(CliConfiguration config)
    {
        var services = new ServiceCollection();

        services.AddMediatR(
            typeof(InventoryService).Assembly,
            typeof(CashRegisterService).Assembly,
            typeof(OrderService).Assembly);

        services.AddSingleton<IInventoryRepository>(
            new MongoInventoryRepository(config.Mongo.ConnectionString, config.Mongo.Database));
        services.AddSingleton<ICashStorage>(new PostgresCashStorage(config.Postgres.ConnectionString));
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IOrderService, OrderService>();

        return services.BuildServiceProvider();
    }
}
