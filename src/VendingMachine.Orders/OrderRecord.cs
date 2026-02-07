namespace VendingMachine.Orders;

public sealed record OrderRecord(
    Guid Id,
    string ProductCode,
    string ProductName,
    decimal Price,
    OrderStatus Status,
    DateTimeOffset CreatedAt);
