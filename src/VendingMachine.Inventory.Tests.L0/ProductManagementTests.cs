using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Inventory.Infrastructure;
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
        var service = BuildInventoryService(repository);
        var product = new Product("COLA", "Coca Cola", 1.50m);

        // Act
        await service.CreateProduct(product);

        // Assert
        var stored = await repository.GetByCodeAsync("COLA");
        Assert.Equal(1.50m, stored.Price);
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var service = BuildInventoryService(repository);
        var product = new Product("COLA", "Coca Cola", -1.00m);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.CreateProduct(product)
        );
    }

    [Fact]
    public async Task UpdateProduct_WithValidPrice_UpdatesProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var service = BuildInventoryService(repository);
        await service.CreateProduct(new Product("WATER", "Water", 1.00m));

        // Act
        await service.UpdateProduct(new Product("WATER", "Water", 1.25m));

        // Assert
        var updated = await repository.GetByCodeAsync("WATER");
        Assert.Equal(1.25m, updated.Price);
    }

    [Fact]
    public async Task UpdateProduct_WithNegativePrice_ThrowsExceptionAndKeepsPrice()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var service = BuildInventoryService(repository);
        await service.CreateProduct(new Product("JUICE", "Orange Juice", 2.00m));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.UpdateProduct(new Product("JUICE", "Orange Juice", -0.50m))
        );

        var stored = await repository.GetByCodeAsync("JUICE");
        Assert.Equal(2.00m, stored.Price);
    }

    [Fact]
    public async Task ReadProduct_ReturnsProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var service = BuildInventoryService(repository);
        await service.CreateProduct(new Product("COLA", "Coca Cola", 1.50m));

        // Act
        var product = await service.GetProductByCode("COLA");

        // Assert
        Assert.Equal("COLA", product.Code);
        Assert.Equal(1.50m, product.Price);
    }

    [Fact]
    public async Task ReadProduct_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var service = BuildInventoryService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await service.GetProductByCode("UNKNOWN")
        );
    }

    [Fact]
    public async Task DeleteProduct_WithZeroStock_RemovesProduct()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var service = BuildInventoryService(repository);
        await service.CreateProduct(new Product("COLA", "Coca Cola", 1.50m));
        await repository.SetStockAsync("COLA", 0);

        // Act
        await service.DeleteProduct("COLA");

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
        var service = BuildInventoryService(repository);
        await service.CreateProduct(new Product("WATER", "Water", 1.00m));
        await repository.SetStockAsync("WATER", 3);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.DeleteProduct("WATER")
        );

        var stored = await repository.GetByCodeAsync("WATER");
        Assert.Equal("WATER", stored.Code);
    }

    private static IInventoryService BuildInventoryService(IInventoryRepository repository)
    {
        var services = new ServiceCollection();
        services.AddSingleton(repository);
        services.AddLogging();
        services.AddMediatR(typeof(InventoryService).Assembly);
        services.AddSingleton<IInventoryService, InventoryService>();
        return services.BuildServiceProvider().GetRequiredService<IInventoryService>();
    }
}
