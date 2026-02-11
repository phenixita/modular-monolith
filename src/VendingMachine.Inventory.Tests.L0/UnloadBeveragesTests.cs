using VendingMachine.Inventory;
using Xunit;

namespace VendingMachine.Inventory.Tests.L0;

[Trait("Level", "L0")]
public sealed class UnloadBeveragesTests
{
    [Theory]
    [InlineData("COLA", 10, 2, 8)]
    [InlineData("COLA", 5, 5, 0)]
    [InlineData("WATER", 20, 15, 5)]
    public async Task UnloadBeverages_DecreasesStock(string productCode, int initialStock, int quantity, int expectedStock)
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new RemoveStockHandler(repository);

        // Setup initial product and stock
        await repository.AddOrUpdateAsync(new Product(productCode, $"{productCode} Name", 1.50m));
        await repository.SetStockAsync(productCode, initialStock);

        // Act
        var command = new RemoveStockCommand(productCode, quantity);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var actualStock = await repository.GetQuantityAsync(productCode);
        Assert.Equal(expectedStock, actualStock);
    }

    [Fact]
    public async Task UnloadBeverages_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new RemoveStockHandler(repository);
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));
        await repository.SetStockAsync("COLA", 3);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(new RemoveStockCommand("COLA", 5), CancellationToken.None)
        );
    }

    [Fact]
    public async Task UnloadBeverages_WithZeroQuantity_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new RemoveStockHandler(repository);
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));
        await repository.SetStockAsync("COLA", 10);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await handler.Handle(new RemoveStockCommand("COLA", 0), CancellationToken.None)
        );
    }

    [Fact]
    public async Task UnloadBeverages_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new RemoveStockHandler(repository);
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));
        await repository.SetStockAsync("COLA", 10);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await handler.Handle(new RemoveStockCommand("COLA", -5), CancellationToken.None)
        );
    }

    [Fact]
    public async Task UnloadBeverages_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new RemoveStockHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await handler.Handle(new RemoveStockCommand("UNKNOWN", 5), CancellationToken.None)
        );
    }
}
