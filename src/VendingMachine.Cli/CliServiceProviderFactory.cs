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
            typeof(CreateProductCommand).Assembly,
            typeof(InsertCashCommand).Assembly,
            typeof(PlaceOrderCommand).Assembly);

        services.AddSingleton<IInventoryRepository>(
            new MongoInventoryRepository(config.Mongo.ConnectionString, config.Mongo.Database));
        services.AddSingleton<ICashStorage>(new PostgresCashStorage(config.Postgres.ConnectionString));
        services.AddScoped<IOrderService, OrderService>();

        return services.BuildServiceProvider();
    }
}
