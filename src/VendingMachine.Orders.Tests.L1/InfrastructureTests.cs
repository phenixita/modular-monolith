using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

[Trait("Level", "L1")]
public sealed class InfrastructureTests
{
    [InfrastructureFact]
    public async Task Purchase_WithSufficientBalance_DispensesProduct_ScalesBalance_AndPersistsOrder()
    {
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var mediator = fixture.Mediator;
            var product = new Product("COLA", "Cola", 1.50m);

            await mediator.Send(new UpsertProductCommand(product));
            await mediator.Send(new SetStockCommand(product.Code, 1));
            await mediator.Send(new InsertCashCommand(2.00m));

            var result = await mediator.Send(new PurchaseProductCommand(product.Code));
            var balance = await mediator.Send(new GetBalanceQuery());
            var remainingStock = await mediator.Send(new GetStockQuery(product.Code));
            var recent = await fixture.OrderRepository.GetRecentAsync(1);

            Assert.Equal(OrderStatus.Success, result.Status);
            Assert.Equal(1.50m, result.ChargedAmount);
            Assert.Equal(0.50m, result.RemainingBalance);
            Assert.Equal(0.50m, balance);
            Assert.Equal(0, remainingStock);

            Assert.Single(recent);
            Assert.Equal("COLA", recent[0].ProductCode);
            Assert.Equal("Cola", recent[0].ProductName);
            Assert.Equal(1.50m, recent[0].Price);
            Assert.Equal(OrderStatus.Success, recent[0].Status);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }
}
