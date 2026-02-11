using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

[Trait("Level", "L1")]
public sealed class OrderInfrastructureTests
{
    [Fact]
    public async Task PlaceOrder_PersistsCashAndInventory()
    {
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var dbName = $"vendingmachine_orders_{Guid.NewGuid():N}";
            var cashStorage = new PostgresCashStorage(fixture.PostgresConnectionString);
            cashStorage.EnsureCreated();
            cashStorage.SetBalance(5.00m);

            var inventory = new MongoInventoryRepository(fixture.MongoConnectionString, dbName);
            await inventory.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
            await inventory.SetStockAsync("COLA", 2);

            var services = BuildServices(cashStorage, inventory);
            var orderService = services.GetRequiredService<IOrderService>();

            var receipt = await orderService.PlaceOrder("COLA");

            Assert.Equal("COLA", receipt.ProductCode);
            Assert.Equal(3.50m, receipt.Balance);
            Assert.Equal(1, receipt.Stock);

            var cashStorage2 = new PostgresCashStorage(fixture.PostgresConnectionString);
            Assert.Equal(3.50m, cashStorage2.GetBalance());

            var inventory2 = new MongoInventoryRepository(fixture.MongoConnectionString, dbName);
            Assert.Equal(1, await inventory2.GetQuantityAsync("COLA"));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    private static ServiceProvider BuildServices(ICashStorage storage, IInventoryRepository repository)
    {
        var services = new ServiceCollection();
        services.AddSingleton(storage);
        services.AddSingleton(repository);
        services.AddMediatR(
            typeof(PlaceOrderCommand).Assembly,
            typeof(ChargeCashCommand).Assembly,
            typeof(GetProductByCodeQuery).Assembly);
        services.AddSingleton<IOrderService, OrderService>();
        return services.BuildServiceProvider();
    }
}
