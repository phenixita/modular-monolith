namespace VendingMachine.Inventory;

public sealed class InventoryCatalog
{
    private readonly Dictionary<string, Product> _products = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Product> GetAll() => _products.Values.OrderBy(product => product.Code);

    public void AddOrUpdate(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products[product.Code] = product;
    }

    public Product GetByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        if (!_products.TryGetValue(code, out var product))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        return product;
    }
}
