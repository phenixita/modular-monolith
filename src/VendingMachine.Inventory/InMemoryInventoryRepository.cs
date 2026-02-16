namespace VendingMachine.Inventory;

internal sealed class InMemoryInventoryRepository : IInventoryRepository
{
    private readonly Dictionary<string, InventoryItem> _items = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = _items.Values
            .Select(item => new Product(item.Code, item.Name, item.Price))
            .ToArray();
        return Task.FromResult<IReadOnlyCollection<Product>>(products);
    }

    public Task<Product> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        if (!_items.TryGetValue(normalized, out var item))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        return Task.FromResult(new Product(item.Code, item.Name, item.Price));
    }

    public Task<int> GetQuantityAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        var quantity = _items.TryGetValue(normalized, out var item) ? item.Quantity : 0;
        return Task.FromResult(quantity);
    }

    public Task AddOrUpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        var normalized = NormalizeCode(product.Code);

        if (_items.TryGetValue(normalized, out var existing))
        {
            _items[normalized] = existing with
            {
                Name = product.Name,
                Price = product.Price
            };
        }
        else
        {
            _items[normalized] = new InventoryItem(product.Code, product.Name, product.Price, 0);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        if (!_items.Remove(normalized))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        return Task.CompletedTask;
    }

    public Task AddStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var normalized = NormalizeCode(code);
        if (!_items.TryGetValue(normalized, out var item))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        _items[normalized] = item with { Quantity = item.Quantity + quantity };
        return Task.CompletedTask;
    }

    public Task RemoveStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var normalized = NormalizeCode(code);
        if (!_items.TryGetValue(normalized, out var item))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        if (item.Quantity < quantity)
        {
            throw new InvalidOperationException("Not enough stock.");
        }

        _items[normalized] = item with { Quantity = item.Quantity - quantity };
        return Task.CompletedTask;
    }

    public Task SetStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be zero or positive.");
        }

        var normalized = NormalizeCode(code);
        if (!_items.TryGetValue(normalized, out var item))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        _items[normalized] = item with { Quantity = quantity };
        return Task.CompletedTask;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    private sealed record InventoryItem(string Code, string Name, decimal Price, int Quantity);
}
