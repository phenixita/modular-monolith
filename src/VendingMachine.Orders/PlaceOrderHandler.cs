using System.Globalization;
using MediatR;
using VendingMachine.Cash;
using VendingMachine.Inventory;

namespace VendingMachine.Orders;

internal sealed class PlaceOrderHandler(IMediator mediator) : IRequestHandler<PlaceOrderCommand, OrderReceipt>
{
    public async Task<OrderReceipt> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeCode(request.Code);
        var product = await FetchProduct(normalizedCode, cancellationToken);
        var currentStock = await FetchCurrentStock(normalizedCode, cancellationToken);
        var currentBalance = await FetchCurrentBalance(cancellationToken);

        EnsureStockIsAvailable(normalizedCode, currentStock);
        EnsureBalanceIsSufficient(product.Price, currentBalance);

        await ChargeCustomerCash(product.Price, cancellationToken);
        await RemoveProductFromStock(normalizedCode, cancellationToken);

        var updatedBalance = await FetchCurrentBalance(cancellationToken);
        var updatedStock = await FetchCurrentStock(normalizedCode, cancellationToken);

        return new OrderReceipt(normalizedCode, product.Price, updatedBalance, updatedStock);
    }

    private async Task<Product> FetchProduct(string code, CancellationToken cancellationToken) =>
        await mediator.Send(new GetProductByCodeQuery(code), cancellationToken);

    private async Task<int> FetchCurrentStock(string code, CancellationToken cancellationToken) =>
        await mediator.Send(new GetStockQuery(code), cancellationToken);

    private async Task<decimal> FetchCurrentBalance(CancellationToken cancellationToken) =>
        await mediator.Send(new GetBalanceQuery(), cancellationToken);

    private async Task ChargeCustomerCash(decimal amount, CancellationToken cancellationToken) =>
        await mediator.Send(new ChargeCashCommand(amount), cancellationToken);

    private async Task RemoveProductFromStock(string code, CancellationToken cancellationToken) =>
        await mediator.Send(new RemoveStockCommand(code, 1), cancellationToken);

    private static void EnsureStockIsAvailable(string code, int stock)
    {
        if (stock <= 0)
        {
            throw new InvalidOperationException($"Product {code} is out of stock.");
        }
    }

    private static void EnsureBalanceIsSufficient(decimal price, decimal balance)
    {
        if (balance < price)
        {
            throw new InvalidOperationException(
                $"Insufficient balance. Price is {FormatMoney(price)}, balance is {FormatMoney(balance)}.");
        }
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    private static string FormatMoney(decimal amount) =>
        amount.ToString("0.00", CultureInfo.InvariantCulture);
}
