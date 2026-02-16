namespace VendingMachine.Inventory;

internal sealed class StockRoom
{
    private readonly Dictionary<string, int> _stock = new(StringComparer.OrdinalIgnoreCase);

    public int GetQuantity(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code is required.", nameof(productCode));
        }

        return _stock.TryGetValue(productCode, out var quantity) ? quantity : 0;
    }

    public void AddStock(string productCode, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code is required.", nameof(productCode));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        _stock[productCode] = GetQuantity(productCode) + quantity;
    }

    public void RemoveStock(string productCode, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code is required.", nameof(productCode));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var available = GetQuantity(productCode);
        if (available < quantity)
        {
            throw new InvalidOperationException("Not enough stock.");
        }

        _stock[productCode] = available - quantity;
    }
}
