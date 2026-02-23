using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Persistence.Abstractions;
using Xunit;

namespace VendingMachine.Orders.Tests.L0;

[Trait("Level", "L0")]
public sealed class OrderPlacementTests
{
    [Fact]
    public async Task PlaceOrder_WithStockAndBalance_ReturnsReceiptAndUpdatesState()
    {
        var storage = new InMemoryCashStorage();
        await storage.SetBalanceAsync(5.00m);

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
        Assert.Equal(3.50m, await storage.GetBalanceAsync());
        Assert.Equal(1, await repository.GetQuantityAsync("COLA"));
    }

    [Fact]
    public async Task PlaceOrder_WithInsufficientBalance_Throws()
    {
        var storage = new InMemoryCashStorage();
        await storage.SetBalanceAsync(0.50m);
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
        await storage.SetBalanceAsync(5.00m);
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
        await storage.SetBalanceAsync(5.00m);
        var repository = new InMemoryInventoryRepository();

        var services = BuildServices(storage, repository);
        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await orderService.PlaceOrder("UNKNOWN")
        );
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
        services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<IInventoryService, InventoryService>();
        services.AddSingleton<ICashRegisterService, CashRegisterService>();
        return services.BuildServiceProvider();
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
            action(cancellationToken);

        public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
            action(cancellationToken);
    }
}
