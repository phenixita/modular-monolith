using System.Globalization;
using MediatR;
using VendingMachine.Cash;
using VendingMachine.Inventory;

namespace VendingMachine.Orders.PlaceOrder;

internal sealed class PlaceOrderHandler(
    IInventoryService inventoryService,
    ICashRegisterService cashRegisterService,
    IOrdersUnitOfWork unitOfWork) : IRequestHandler<PlaceOrderCommand, OrderReceipt>
{
    public async Task<OrderReceipt> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteAsync(async ct =>
        {
            var normalizedCode = NormalizeCode(request.Code);
            var product = await FetchProduct(normalizedCode, ct);
            var currentStock = await FetchCurrentStock(normalizedCode, ct);
            var currentBalance = await FetchCurrentBalance(ct);

            EnsureStockIsAvailable(normalizedCode, currentStock);
            EnsureBalanceIsSufficient(product.Price, currentBalance);

            await ChargeCustomerCash(product.Price, ct);
            await RemoveProductFromStock(normalizedCode, ct);

            var updatedBalance = await FetchCurrentBalance(ct);
            var updatedStock = await FetchCurrentStock(normalizedCode, ct);

            return new OrderReceipt(normalizedCode, product.Price, updatedBalance, updatedStock);
        }, cancellationToken);
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
