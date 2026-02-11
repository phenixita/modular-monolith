using System.Globalization;
using MediatR;
using VendingMachine.Cash;
using VendingMachine.Inventory;

namespace VendingMachine.Orders;

public sealed class PlaceOrderHandler(IMediator mediator) : IRequestHandler<PlaceOrderCommand, OrderReceipt>
{
    public async Task<OrderReceipt> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeCode(request.Code);
        var product = await mediator.Send(new GetProductByCodeQuery(normalizedCode), cancellationToken);
        var stock = await mediator.Send(new GetStockQuery(normalizedCode), cancellationToken);
        var balance = await mediator.Send(new GetBalanceQuery(), cancellationToken);

        if (stock <= 0)
        {
            throw new InvalidOperationException($"Product {normalizedCode} is out of stock.");
        }

        if (balance < product.Price)
        {
            throw new InvalidOperationException(
                $"Insufficient balance. Price is {FormatMoney(product.Price)}, balance is {FormatMoney(balance)}.");
        }

        await mediator.Send(new ChargeCashCommand(product.Price), cancellationToken);
        await mediator.Send(new RemoveStockCommand(normalizedCode, 1), cancellationToken);

        var updatedBalance = await mediator.Send(new GetBalanceQuery(), cancellationToken);
        var updatedStock = await mediator.Send(new GetStockQuery(normalizedCode), cancellationToken);

        return new OrderReceipt(normalizedCode, product.Price, updatedBalance, updatedStock);
    }

    private static string FormatMoney(decimal amount) =>
        amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }
}
