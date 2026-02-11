namespace VendingMachine.Inventory;

public interface IInventoryRepository
{
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<int> GetQuantityAsync(string code, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(string code, CancellationToken cancellationToken = default);
    Task AddStockAsync(string code, int quantity, CancellationToken cancellationToken = default);
    Task RemoveStockAsync(string code, int quantity, CancellationToken cancellationToken = default);
    Task SetStockAsync(string code, int quantity, CancellationToken cancellationToken = default);
}
