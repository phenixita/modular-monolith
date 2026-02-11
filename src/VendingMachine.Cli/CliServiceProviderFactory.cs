using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;

internal static class CliServiceProviderFactory
{
    public static ServiceProvider Build(CliConfiguration config)
    {
        var services = new ServiceCollection();

        services.AddMediatR(
            typeof(CreateProductCommand).Assembly,
            typeof(InsertCashCommand).Assembly);

        services.AddSingleton<IInventoryRepository>(
            new MongoInventoryRepository(config.Mongo.ConnectionString, config.Mongo.Database));
        services.AddSingleton<ICashStorage>(new PostgresCashStorage(config.Postgres.ConnectionString));

        return services.BuildServiceProvider();
    }
}
