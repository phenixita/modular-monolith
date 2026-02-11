using MediatR;
using VendingMachine.Cash;
using VendingMachine.Inventory;

namespace VendingMachine.Orders;

public sealed record PurchaseProductCommand(string ProductCode) : IRequest<OrderResult>;

public sealed class PurchaseProductHandler(IMediator mediator, IOrderRepository orders)
    : IRequestHandler<PurchaseProductCommand, OrderResult>
{
    public async Task<OrderResult> Handle(PurchaseProductCommand request, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductByCodeQuery(request.ProductCode), cancellationToken);
        var stock = await mediator.Send(new GetStockQuery(product.Code), cancellationToken);

        if (stock <= 0)
        {
            var balance = await mediator.Send(new GetBalanceQuery(), cancellationToken);
            var outOfStock = OrderResult.OutOfStock(product, balance);
            await orders.AddAsync(CreateOrder(outOfStock), cancellationToken);
            return outOfStock;
        }

        try
        {
            await mediator.Send(new ChargeCashCommand(product.Price), cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var balance = await mediator.Send(new GetBalanceQuery(), cancellationToken);
            var insufficient = OrderResult.InsufficientFunds(product, balance);
            await orders.AddAsync(CreateOrder(insufficient), cancellationToken);
            return insufficient;
        }

        await mediator.Send(new RemoveStockCommand(product.Code, 1), cancellationToken);
        var remainingBalance = await mediator.Send(new GetBalanceQuery(), cancellationToken);
        var success = OrderResult.Success(product, remainingBalance);
        await orders.AddAsync(CreateOrder(success), cancellationToken);
        return success;
    }

    private static OrderRecord CreateOrder(OrderResult result) =>
        new(Guid.NewGuid(), result.Product.Code, result.Product.Name, result.Product.Price, result.Status,
            DateTimeOffset.UtcNow);
}
