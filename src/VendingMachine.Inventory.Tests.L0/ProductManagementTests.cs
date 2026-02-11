using VendingMachine.Inventory;
using Xunit;

namespace VendingMachine.Inventory.Tests.L0;

[Trait("Level", "L0")]
public sealed class ProductManagementTests
{
    [Fact]
    public async Task CreateProduct_WithValidPrice_CreatesProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new CreateProductHandler(repository);
        var product = new Product("COLA", "Coca Cola", 1.50m);

        // Act
        await handler.Handle(new CreateProductCommand(product), CancellationToken.None);

        // Assert
        var stored = await repository.GetByCodeAsync("COLA");
        Assert.Equal(1.50m, stored.Price);
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new CreateProductHandler(repository);
        var product = new Product("COLA", "Coca Cola", -1.00m);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await handler.Handle(new CreateProductCommand(product), CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateProduct_WithValidPrice_UpdatesProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("WATER", "Water", 1.00m));
        var handler = new UpdateProductHandler(repository);

        // Act
        await handler.Handle(new UpdateProductCommand(new Product("WATER", "Water", 1.25m)), CancellationToken.None);

        // Assert
        var updated = await repository.GetByCodeAsync("WATER");
        Assert.Equal(1.25m, updated.Price);
    }

    [Fact]
    public async Task UpdateProduct_WithNegativePrice_ThrowsExceptionAndKeepsPrice()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("JUICE", "Orange Juice", 2.00m));
        var handler = new UpdateProductHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await handler.Handle(new UpdateProductCommand(new Product("JUICE", "Orange Juice", -0.50m)),
                CancellationToken.None)
        );

        var stored = await repository.GetByCodeAsync("JUICE");
        Assert.Equal(2.00m, stored.Price);
    }

    [Fact]
    public async Task ReadProduct_ReturnsProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        var handler = new GetProductByCodeHandler(repository);

        // Act
        var product = await handler.Handle(new GetProductByCodeQuery("COLA"), CancellationToken.None);

        // Assert
        Assert.Equal("COLA", product.Code);
        Assert.Equal(1.50m, product.Price);
    }

    [Fact]
    public async Task ReadProduct_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new GetProductByCodeHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await handler.Handle(new GetProductByCodeQuery("UNKNOWN"), CancellationToken.None)
        );
    }

    [Fact]
    public async Task DeleteProduct_WithZeroStock_RemovesProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await repository.SetStockAsync("COLA", 0);
        var handler = new DeleteProductHandler(repository);

        // Act
        await handler.Handle(new DeleteProductCommand("COLA"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await repository.GetByCodeAsync("COLA")
        );
    }

    [Fact]
    public async Task DeleteProduct_WithStock_ThrowsExceptionAndKeepsProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        await repository.AddOrUpdateAsync(new Product("WATER", "Water", 1.00m));
        await repository.SetStockAsync("WATER", 3);
        var handler = new DeleteProductHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(new DeleteProductCommand("WATER"), CancellationToken.None)
        );

        var stored = await repository.GetByCodeAsync("WATER");
        Assert.Equal("WATER", stored.Code);
    }
}
