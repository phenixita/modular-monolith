using MediatR;
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

        var configWrapper = new CliConfigurationWrapper(config);
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configWrapper);

        services.AddPersistence();
        services.AddVendingMachineInventoryModule();
        services.AddCashRegisterModule();
        services.AddOrdersModule(); 

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddSingleton<IInventoryRepository>(
            new MongoInventoryRepository(config.Mongo.ConnectionString, config.Mongo.Database));
        
        services.AddScoped<ICashStorage>(sp =>
        {
            var connectionFactory = sp.GetRequiredService<IDbConnectionFactory>();
            return new PostgresCashStorage(connectionFactory);
        });
        
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IOrderService, OrderService>();

        return services.BuildServiceProvider();
    }
}
