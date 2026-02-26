using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using Xunit;

namespace VendingMachine.Orders.Tests.L0;

[Trait("Level", "L0")]
public sealed class OrderPlacementTests
{
    [Fact]
    public async Task PlaceOrder_WithStockAndBalance_ReturnsReceiptAndUpdatesState()
    {
        var services = BuildServices();
        var cashRepository = services.GetRequiredService<ICashRepository>();
        await cashRepository.SetBalanceAsync(5.00m);

        var inventoryRepository = services.GetRequiredService<IInventoryRepository>();
        await inventoryRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await inventoryRepository.SetStockAsync("COLA", 2);

        var orderService = services.GetRequiredService<IOrderService>();

        var receipt = await orderService.PlaceOrder("cola");

        Assert.Equal("COLA", receipt.ProductCode);
        Assert.Equal(1.50m, receipt.Price);
        Assert.Equal(3.50m, receipt.Balance);
        Assert.Equal(1, receipt.Stock);
        Assert.Equal(3.50m, await cashRepository.GetBalanceAsync());
        Assert.Equal(1, await inventoryRepository.GetQuantityAsync("COLA"));
    }

    [Fact]
    public async Task PlaceOrder_WithInsufficientBalance_Throws()
    {
        var services = BuildServices();
        var cashRepository = services.GetRequiredService<ICashRepository>();
        await cashRepository.SetBalanceAsync(0.50m);
        var inventoryRepository = services.GetRequiredService<IInventoryRepository>();
        await inventoryRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await inventoryRepository.SetStockAsync("COLA", 1);

        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await orderService.PlaceOrder("COLA")
        );
    }

    [Fact]
    public async Task PlaceOrder_WithNoStock_Throws()
    {
        var services = BuildServices();
        var cashRepository = services.GetRequiredService<ICashRepository>();
        await cashRepository.SetBalanceAsync(5.00m);
        var inventoryRepository = services.GetRequiredService<IInventoryRepository>();
        await inventoryRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await inventoryRepository.SetStockAsync("COLA", 0);

        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await orderService.PlaceOrder("COLA")
        );
    }

    [Fact]
    public async Task PlaceOrder_WithUnknownProduct_Throws()
    {
        var services = BuildServices();
        var cashrepository = services.GetRequiredService<ICashRepository>();
        await cashrepository.SetBalanceAsync(5.00m);

        var orderService = services.GetRequiredService<IOrderService>();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await orderService.PlaceOrder("UNKNOWN")
        );
    }

    [Fact]
    public async Task PlaceOrder_WithStockAndBalance_PublishesOrderConfirmedEvent()
    {
        var services = BuildServices();
        var cashRepository = services.GetRequiredService<ICashRepository>();
        await cashRepository.SetBalanceAsync(5.00m);

        var inventoryRepository = services.GetRequiredService<IInventoryRepository>();
        await inventoryRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await inventoryRepository.SetStockAsync("COLA", 2);

        var orderService = services.GetRequiredService<IOrderService>();
        var eventCollector = services.GetRequiredService<OrderConfirmedEventCollector>();

        await orderService.PlaceOrder("cola");

        var publishedEvent = Assert.Single(eventCollector.Events);
        Assert.Equal("COLA", publishedEvent.ProductCode);
        Assert.Equal(1.50m, publishedEvent.Price);
        Assert.True(publishedEvent.OrderedAt <= DateTimeOffset.UtcNow);
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCashRegisterModuleForTests();
        services.AddInventoryModuleForTests();
        services.AddOrdersModuleForTests();

        services.AddSingleton<OrderConfirmedEventCollector>();
        services.AddSingleton<INotificationHandler<OrderConfirmed>>(sp => sp.GetRequiredService<OrderConfirmedEventCollector>());

        return services.BuildServiceProvider();
    }

    private sealed class OrderConfirmedEventCollector : INotificationHandler<OrderConfirmed>
    {
        public List<OrderConfirmed> Events { get; } = [];

        public Task Handle(OrderConfirmed notification, CancellationToken cancellationToken)
        {
            Events.Add(notification);
            return Task.CompletedTask;
        }
    }
}
