namespace VendingMachine.Inventory;

public interface IInventoryService
{
    Task CreateProduct(Product product, CancellationToken cancellationToken = default);
    Task UpdateProduct(Product product, CancellationToken cancellationToken = default);
    Task DeleteProduct(string code, CancellationToken cancellationToken = default);
    Task UpsertProduct(Product product, CancellationToken cancellationToken = default);
    Task<Product> GetProductByCode(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> ListProducts(CancellationToken cancellationToken = default);
    Task AddStock(string code, int quantity, CancellationToken cancellationToken = default);
    Task RemoveStock(string code, int quantity, CancellationToken cancellationToken = default);
    Task SetStock(string code, int quantity, CancellationToken cancellationToken = default);
    Task<int> GetStock(string code, CancellationToken cancellationToken = default);
}
