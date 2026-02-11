using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using Xunit;

namespace VendingMachine.Orders.Tests.L0;

[Trait("Level", "L0")]
public sealed class OrderServiceTests
{
    [Fact]
    public void Purchase_ReturnsSuccess_WhenEnoughBalanceAndStock()
    {
        var cashRegister = BuildRegister(new InMemoryCashStorage());
        var catalog = new InventoryCatalog();
        var stockRoom = new StockRoom();
        var service = new OrderService(cashRegister, catalog, stockRoom);

        catalog.AddOrUpdate(new Product("C01", "Espresso", 1.20m));
        stockRoom.AddStock("C01", 2);
        cashRegister.Insert(2.00m);

        var result = service.Purchase("C01");

        Assert.Equal(OrderStatus.Success, result.Status);
        Assert.Equal(1.20m, result.ChargedAmount);
        Assert.Equal(0.80m, result.RemainingBalance);
        Assert.Null(result.Message);
        Assert.Equal(1, stockRoom.GetQuantity("C01"));
        Assert.Equal(0.80m, cashRegister.Balance);
    }

    [Fact]
    public void Purchase_ReturnsOutOfStock_WhenNoInventory()
    {
        var cashRegister = BuildRegister(new InMemoryCashStorage());
        var catalog = new InventoryCatalog();
        var stockRoom = new StockRoom();
        var service = new OrderService(cashRegister, catalog, stockRoom);

        catalog.AddOrUpdate(new Product("S01", "Chips", 1.30m));
        cashRegister.Insert(2.00m);

        var result = service.Purchase("S01");

        Assert.Equal(OrderStatus.OutOfStock, result.Status);
        Assert.Equal(0m, result.ChargedAmount);
        Assert.Equal(2.00m, result.RemainingBalance);
        Assert.Equal(2.00m, cashRegister.Balance);
    }

    [Fact]
    public void Purchase_ReturnsInsufficientFunds_WhenBalanceTooLow()
    {
        var cashRegister = BuildRegister(new InMemoryCashStorage());
        var catalog = new InventoryCatalog();
        var stockRoom = new StockRoom();
        var service = new OrderService(cashRegister, catalog, stockRoom);

        catalog.AddOrUpdate(new Product("B01", "Water", 1.00m));
        stockRoom.AddStock("B01", 1);

        var result = service.Purchase("B01");

        Assert.Equal(OrderStatus.InsufficientFunds, result.Status);
        Assert.Equal(0m, result.ChargedAmount);
        Assert.Equal(0m, result.RemainingBalance);
        Assert.Equal(1, stockRoom.GetQuantity("B01"));
    }

    private static ICashRegisterService BuildRegister(ICashStorage storage)
    {
        var services = new ServiceCollection();
        services.AddSingleton(storage);
        services.AddMediatR(typeof(CashRegisterService).Assembly);
        services.AddSingleton<CashRegisterService>();
        return services.BuildServiceProvider().GetRequiredService<CashRegisterService>();
    }
}
