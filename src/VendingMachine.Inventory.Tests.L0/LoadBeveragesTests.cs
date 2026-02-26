using Microsoft.Extensions.DependencyInjection;
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
        var (service, repository) = BuildInventoryService();

        // Setup initial product and stock
        await repository.AddOrUpdateAsync(new Product(productCode, $"{productCode} Name", 1.50m));
        if (initialStock > 0)
        {
            await repository.SetStockAsync(productCode, initialStock);
        }

        // Act
        await service.AddStock(productCode, quantity);

        // Assert
        var actualStock = await repository.GetQuantityAsync(productCode);
        Assert.Equal(expectedStock, actualStock);
    }

    [Fact]
    public async Task LoadBeverages_WithZeroQuantity_ThrowsException()
    {
        // Arrange
        var (service, repository) = BuildInventoryService();
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.AddStock("COLA", 0)
        );
    }

    [Fact]
    public async Task LoadBeverages_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var (service, repository) = BuildInventoryService();
        await repository.AddOrUpdateAsync(new Product("COLA", "Cola", 1.50m));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.AddStock("COLA", -5)
        );
    }

    [Fact]
    public async Task LoadBeverages_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var (service, _) = BuildInventoryService();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await service.AddStock("UNKNOWN", 5)
        );
    }

    private static (IInventoryService Service, IInventoryRepository Repository) BuildInventoryService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInventoryModuleForTests();

        var serviceProvider = services.BuildServiceProvider();
        return (
            serviceProvider.GetRequiredService<IInventoryService>(),
            serviceProvider.GetRequiredService<IInventoryRepository>()
        );
    }
}
