using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace VendingMachine.Inventory;

public sealed class MongoInventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<InventoryItemDocument> _items;

    public MongoInventoryRepository(string connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name is required.", nameof(databaseName));
        }

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _items = database.GetCollection<InventoryItemDocument>("inventory_items");
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _items.Find(FilterDefinition<InventoryItemDocument>.Empty)
            .SortBy(item => item.Code)
            .ToListAsync(cancellationToken);

        return items.Select(item => new Product(item.Code, item.Name, item.Price)).ToArray();
    }

    public async Task<Product> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        var item = await _items.Find(document => document.Code == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        return new Product(item.Code, item.Name, item.Price);
    }

    public async Task<int> GetQuantityAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        var item = await _items.Find(document => document.Code == normalized)
            .Project(document => document.Quantity)
            .FirstOrDefaultAsync(cancellationToken);

        return item;
    }

    public Task AddOrUpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        var normalized = NormalizeCode(product.Code);

        var filter = Builders<InventoryItemDocument>.Filter.Eq(document => document.Code, normalized);
        var update = Builders<InventoryItemDocument>.Update
            .Set(document => document.Code, normalized)
            .Set(document => document.Name, product.Name)
            .Set(document => document.Price, product.Price)
            .SetOnInsert(document => document.Quantity, 0);

        return _items.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task AddStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var normalized = NormalizeCode(code);
        var filter = Builders<InventoryItemDocument>.Filter.Eq(document => document.Code, normalized);
        var update = Builders<InventoryItemDocument>.Update.Inc(document => document.Quantity, quantity);
        var result = await _items.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }
    }

    public async Task RemoveStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var normalized = NormalizeCode(code);
        var item = await _items.Find(document => document.Code == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        var available = item?.Quantity ?? 0;
        if (available < quantity)
        {
            throw new InvalidOperationException("Not enough stock.");
        }

        var update = Builders<InventoryItemDocument>.Update.Inc(document => document.Quantity, -quantity);
        await _items.UpdateOneAsync(document => document.Code == normalized, update, cancellationToken: cancellationToken);
    }

    public async Task SetStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be zero or positive.");
        }

        var normalized = NormalizeCode(code);
        var filter = Builders<InventoryItemDocument>.Filter.Eq(document => document.Code, normalized);
        var update = Builders<InventoryItemDocument>.Update.Set(document => document.Quantity, quantity);
        var result = await _items.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
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

    private sealed class InventoryItemDocument
    {
        [BsonId]
        public string Code { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public int Quantity { get; init; }
    }
}
