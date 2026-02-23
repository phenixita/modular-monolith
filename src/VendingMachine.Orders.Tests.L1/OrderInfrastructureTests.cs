using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Orders;
using VendingMachine.Orders.PlaceOrder;
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
            var factory = new TestDbConnectionFactory(fixture.PostgresConnectionString);
            var cashStorage = new PostgresCashStorage(factory);
            await cashStorage.EnsureCreatedAsync();
            await cashStorage.SetBalanceAsync(5.00m);

            var inventory = new MongoInventoryRepository(fixture.MongoConnectionString, dbName);
            await inventory.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
            await inventory.SetStockAsync("COLA", 2);

            var services = BuildServices(cashStorage, inventory);
            var orderService = services.GetRequiredService<IOrderService>();

            var receipt = await orderService.PlaceOrder("COLA");

            Assert.Equal("COLA", receipt.ProductCode);
            Assert.Equal(3.50m, receipt.Balance);
            Assert.Equal(1, receipt.Stock);

            var cashStorage2 = new PostgresCashStorage(new TestDbConnectionFactory(fixture.PostgresConnectionString));
            Assert.Equal(3.50m, await cashStorage2.GetBalanceAsync());

            var inventory2 = new MongoInventoryRepository(fixture.MongoConnectionString, dbName);
            Assert.Equal(1, await inventory2.GetQuantityAsync("COLA"));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task PlaceOrderSaga_WhenStockUpdateFails_CompensatesCashAndKeepsInventoryUnchanged()
    {
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var dbName = $"vendingmachine_orders_{Guid.NewGuid():N}";
            var factory = new TestDbConnectionFactory(fixture.PostgresConnectionString);
            var cashStorage = new PostgresCashStorage(factory);
            await cashStorage.EnsureCreatedAsync();
            await cashStorage.SetBalanceAsync(5.00m);

            var mongoRepository = new MongoInventoryRepository(fixture.MongoConnectionString, dbName);
            await mongoRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
            await mongoRepository.SetStockAsync("COLA", 2);
            var failingRepository = new FailingOnRemoveStockRepository(mongoRepository);

            var services = BuildServices(cashStorage, failingRepository);
            var mediator = services.GetRequiredService<IMediator>();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await mediator.Send(new PlaceOrderSagaCommand("COLA"))
            );

            Assert.Contains("Charged cash has been refunded", exception.Message);

            var cashStorage2 = new PostgresCashStorage(new TestDbConnectionFactory(fixture.PostgresConnectionString));
            Assert.Equal(5.00m, await cashStorage2.GetBalanceAsync());

            var inventory2 = new MongoInventoryRepository(fixture.MongoConnectionString, dbName);
            Assert.Equal(2, await inventory2.GetQuantityAsync("COLA"));
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
        services.AddLogging();
        services.AddMediatR(
            typeof(OrderService).Assembly,
            typeof(CashRegisterService).Assembly,
            typeof(InventoryService).Assembly);
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<IInventoryService, InventoryService>();
        services.AddSingleton<ICashRegisterService, CashRegisterService>();
        return services.BuildServiceProvider();
    }

    private sealed class FailingOnRemoveStockRepository(IInventoryRepository innerRepository) : IInventoryRepository
    {
        public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
            innerRepository.GetAllAsync(cancellationToken);

        public Task<Product> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            innerRepository.GetByCodeAsync(code, cancellationToken);

        public Task<int> GetQuantityAsync(string code, CancellationToken cancellationToken = default) =>
            innerRepository.GetQuantityAsync(code, cancellationToken);

        public Task AddOrUpdateAsync(Product product, CancellationToken cancellationToken = default) =>
            innerRepository.AddOrUpdateAsync(product, cancellationToken);

        public Task DeleteAsync(string code, CancellationToken cancellationToken = default) =>
            innerRepository.DeleteAsync(code, cancellationToken);

        public Task AddStockAsync(string code, int quantity, CancellationToken cancellationToken = default) =>
            innerRepository.AddStockAsync(code, quantity, cancellationToken);

        public Task RemoveStockAsync(string code, int quantity, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated stock persistence failure.");

        public Task SetStockAsync(string code, int quantity, CancellationToken cancellationToken = default) =>
            innerRepository.SetStockAsync(code, quantity, cancellationToken);
    }
}
