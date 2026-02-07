using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

public sealed class InfrastructureTests
{
    [InfrastructureFact]
    public async Task Purchase_PersistsOrderAndAdjustsStock()
    {
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var mediator = fixture.Mediator;
            var product = new Product("T01", "Test Coffee", 1.10m);

            await mediator.Send(new UpsertProductCommand(product));
            await mediator.Send(new SetStockCommand(product.Code, 2));
            await mediator.Send(new InsertCashCommand(2.00m));

            var result = await mediator.Send(new PurchaseProductCommand(product.Code));

            Assert.Equal(OrderStatus.Success, result.Status);
            var remaining = await mediator.Send(new GetStockQuery(product.Code));
            Assert.Equal(1, remaining);

            var recent = await fixture.OrderRepository.GetRecentAsync(1);
            Assert.Single(recent);
            Assert.Equal(OrderStatus.Success, recent[0].Status);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }
}
