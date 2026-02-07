using VendingMachine.Inventory;

namespace VendingMachine.Orders;

public sealed record OrderResult(Product Product, OrderStatus Status, string? Message)
{
    public static OrderResult Success(Product product) =>
        new(product, OrderStatus.Success, null);

    public static OrderResult OutOfStock(Product product) =>
        new(product, OrderStatus.OutOfStock, "Product is out of stock.");

    public static OrderResult InsufficientFunds(Product product, decimal balance) =>
        new(product, OrderStatus.InsufficientFunds,
            $"Insufficient balance. Insert at least {product.Price - balance:0.00}.");
}
