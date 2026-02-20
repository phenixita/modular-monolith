using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Orders;
using VendingMachine.Orders.PlaceOrder;
using Xunit;

namespace VendingMachine.Orders.Tests.L0;

[Trait("Level", "L0")]
public sealed class OrderPlacementTests
{
    [Fact]
    public async Task PlaceOrder_WithStockAndBalance_ReturnsReceiptAndUpdatesState()
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(5.00m);

        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await repository.SetStockAsync("COLA", 2);

        var services = BuildServices(storage, repository);
        var orderService = services.GetRequiredService<IOrderService>();

        var receipt = await orderService.PlaceOrder("cola");

        Assert.Equal("COLA", receipt.ProductCode);
        Assert.Equal(1.50m, receipt.Price);
        Assert.Equal(3.50m, receipt.Balance);
        Assert.Equal(1, receipt.Stock);
        Assert.Equal(3.50m, storage.GetBalance());
        Assert.Equal(1, await repository.GetQuantityAsync("COLA"));
    }

    [Fact]
    public async Task PlaceOrder_WithInsufficientBalance_Throws()
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(0.50m);
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await repository.SetStockAsync("COLA", 1);

        var services = BuildServices(storage, repository);
        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await orderService.PlaceOrder("COLA")
        );
    }

    [Fact]
    public async Task PlaceOrder_WithNoStock_Throws()
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(5.00m);
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await repository.SetStockAsync("COLA", 0);

        var services = BuildServices(storage, repository);
        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await orderService.PlaceOrder("COLA")
        );
    }

    [Fact]
    public async Task PlaceOrder_WithUnknownProduct_Throws()
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(5.00m);
        var repository = new InMemoryInventoryRepository();

        var services = BuildServices(storage, repository);
        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await orderService.PlaceOrder("UNKNOWN")
        );
    }

    [Fact]
    public async Task PlaceOrderSaga_WhenStockUpdateFails_RefundsChargedCashAndThrowsCompensatedError()
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(5.00m);
        var baseRepository = new InMemoryInventoryRepository();
        await baseRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await baseRepository.SetStockAsync("COLA", 2);
        var repository = new FailingOnRemoveStockRepository(baseRepository);

        var services = BuildServices(storage, repository);
        var mediator = services.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await mediator.Send(new PlaceOrderSagaCommand("COLA"))
        );

        Assert.Contains("Charged cash has been refunded", exception.Message);
        Assert.Equal(5.00m, storage.GetBalance());
        Assert.Equal(2, await baseRepository.GetQuantityAsync("COLA"));
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
