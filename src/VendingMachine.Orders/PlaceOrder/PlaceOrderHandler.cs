using System.Globalization;
using MediatR;
using VendingMachine.Cash;
using VendingMachine.Inventory;

namespace VendingMachine.Orders.PlaceOrder;

internal sealed class PlaceOrderHandler(
    IInventoryService inventoryService,
    ICashRegisterService cashRegisterService) : IRequestHandler<PlaceOrderCommand, OrderReceipt>
{
    public async Task<OrderReceipt> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeCode(request.Code);
        var product = await FetchProduct(normalizedCode, cancellationToken);
        var currentStock = await FetchCurrentStock(normalizedCode, cancellationToken);
        var currentBalance = await FetchCurrentBalance(cancellationToken);

        EnsureStockIsAvailable(normalizedCode, currentStock);
        EnsureBalanceIsSufficient(product.Price, currentBalance);

        // Execute operations with compensating transaction pattern
        await ChargeCustomerCash(product.Price, cancellationToken);
        
        try
        {
            await RemoveProductFromStock(normalizedCode, cancellationToken);
        }
        catch
        {
            // Compensate: refund the charged amount if inventory operation fails
            await cashRegisterService.Insert(product.Price);
            throw;
        }

        var updatedBalance = await FetchCurrentBalance(cancellationToken);
        var updatedStock = await FetchCurrentStock(normalizedCode, cancellationToken);

        return new OrderReceipt(normalizedCode, product.Price, updatedBalance, updatedStock);
    }

    private async Task<Product> FetchProduct(string code, CancellationToken cancellationToken) =>
        await inventoryService.GetProductByCode(code);

    private async Task<int> FetchCurrentStock(string code, CancellationToken cancellationToken) =>
        await inventoryService.GetStock(code);

    private async Task<decimal> FetchCurrentBalance(CancellationToken cancellationToken) =>
        await cashRegisterService.GetBalance();

    private async Task ChargeCustomerCash(decimal amount, CancellationToken cancellationToken) =>
        await cashRegisterService.Charge(amount);

    private async Task RemoveProductFromStock(string code, CancellationToken cancellationToken) =>
        await inventoryService.RemoveStock(code, 1);

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
