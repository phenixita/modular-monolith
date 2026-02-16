namespace VendingMachine.Orders;

public sealed record OrderReceipt(string ProductCode, decimal Price, decimal Balance, int Stock);
