namespace VendingMachine.Inventory;

public interface IInventoryService
{
    Task CreateProduct(Product product);
    Task UpdateProduct(Product product);
    Task DeleteProduct(string code);
    Task UpsertProduct(Product product);
    Task<Product> GetProductByCode(string code);
    Task<IReadOnlyCollection<Product>> ListProducts();
    Task AddStock(string code, int quantity);
    Task RemoveStock(string code, int quantity);
    Task SetStock(string code, int quantity);
    Task<int> GetStock(string code);
}
