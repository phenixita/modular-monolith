using VendingMachine.Inventory;

namespace VendingMachine.Orders;

public sealed record OrderResult(
    Product Product,
    OrderStatus Status,
    string? Message,
    decimal ChargedAmount,
    decimal RemainingBalance)
{
    public static OrderResult Success(Product product, decimal remainingBalance) =>
        new(product, OrderStatus.Success, null, product.Price, remainingBalance);

    public static OrderResult OutOfStock(Product product, decimal balance) =>
        new(product, OrderStatus.OutOfStock, "Product is out of stock.", 0m, balance);

    public static OrderResult InsufficientFunds(Product product, decimal balance) =>
        new(product, OrderStatus.InsufficientFunds,
            $"Insufficient balance. Insert at least {product.Price - balance:0.00}.", 0m, balance);
}
