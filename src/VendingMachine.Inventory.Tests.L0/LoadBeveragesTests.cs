using VendingMachine.Inventory;
using Xunit;

namespace VendingMachine.Inventory.Tests.L0;

[Trait("Level", "L0")]
public sealed class LoadBeveragesTests
{
    [Theory]
    [InlineData("COLA", 0, 5, 5)]
    [InlineData("COLA", 5, 3, 8)]
    [InlineData("WATER", 10, 10, 20)]
    public async Task LoadBeverages_IncreasesStock(string productCode, int initialStock, int quantity, int expectedStock)
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new AddStockHandler(repository);

        // Setup initial product and stock
        await repository.AddOrUpdateAsync(new Product(productCode, $"{productCode} Name", 1.50m));
        if (initialStock > 0)
        {
            await repository.SetStockAsync(productCode, initialStock);
        }

        // Act
        var command = new AddStockCommand(productCode, quantity);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var actualStock = await repository.GetQuantityAsync(productCode);
        Assert.Equal(expectedStock, actualStock);
    }

    [Fact]
    public async Task LoadBeverages_WithZeroQuantity_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new AddStockHandler(repository);
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await handler.Handle(new AddStockCommand("COLA", 0), CancellationToken.None)
        );
    }

    [Fact]
    public async Task LoadBeverages_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new AddStockHandler(repository);
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await handler.Handle(new AddStockCommand("COLA", -5), CancellationToken.None)
        );
    }

    [Fact]
    public async Task LoadBeverages_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var repository = new InMemoryInventoryRepository();
        var handler = new AddStockHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await handler.Handle(new AddStockCommand("UNKNOWN", 5), CancellationToken.None)
        );
    }
}
