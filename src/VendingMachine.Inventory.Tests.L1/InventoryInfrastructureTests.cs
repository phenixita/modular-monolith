using VendingMachine.Inventory;
using Xunit;

namespace VendingMachine.Inventory.Tests.L1;

[Trait("Level", "L1")]
public sealed class InventoryInfrastructureTests
{
    [Fact]
    public async Task AddStock_PersistsDataInMongoDB()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");

            // Create a product first
            var product = new Product("COLA", "Coca Cola", 1.50m);
            await repository1.AddOrUpdateAsync(product);

            // Act - Add stock using first repository instance
            await repository1.AddStockAsync("COLA", 5);

            // Assert - Verify persistence by reading from a new repository instance
            var repository2 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");
            var quantity = await repository2.GetQuantityAsync("COLA");
            Assert.Equal(5, quantity);

            // Act - Add more stock
            await repository2.AddStockAsync("COLA", 3);

            // Assert - Verify the total is correct
            var repository3 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");
            var finalQuantity = await repository3.GetQuantityAsync("COLA");
            Assert.Equal(8, finalQuantity);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task RemoveStock_PersistsDataInMongoDB()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");

            // Create a product and set initial stock
            var product = new Product("WATER", "Water", 1.00m);
            await repository1.AddOrUpdateAsync(product);
            await repository1.SetStockAsync("WATER", 20);

            // Act - Remove stock using first repository instance
            await repository1.RemoveStockAsync("WATER", 15);

            // Assert - Verify persistence by reading from a new repository instance
            var repository2 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");
            var quantity = await repository2.GetQuantityAsync("WATER");
            Assert.Equal(5, quantity);

            // Act - Remove more stock
            await repository2.RemoveStockAsync("WATER", 5);

            // Assert - Verify the final quantity is zero
            var repository3 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");
            var finalQuantity = await repository3.GetQuantityAsync("WATER");
            Assert.Equal(0, finalQuantity);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddStock_WithMultipleProducts_PersistsCorrectly()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");

            // Create multiple products
            await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
            await repository.AddOrUpdateAsync(new Product("WATER", "Water", 1.00m));
            await repository.AddOrUpdateAsync(new Product("JUICE", "Orange Juice", 2.00m));

            // Act - Add stock to each product
            await repository.AddStockAsync("COLA", 10);
            await repository.AddStockAsync("WATER", 15);
            await repository.AddStockAsync("JUICE", 8);

            // Assert - Verify each product has correct stock
            var repository2 = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");
            Assert.Equal(10, await repository2.GetQuantityAsync("COLA"));
            Assert.Equal(15, await repository2.GetQuantityAsync("WATER"));
            Assert.Equal(8, await repository2.GetQuantityAsync("JUICE"));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task RemoveStock_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");

            await repository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
            await repository.SetStockAsync("COLA", 3);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await repository.RemoveStockAsync("COLA", 5)
            );
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddStock_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");

            // Act & Assert - Try to add stock to a product that doesn't exist
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await repository.AddStockAsync("UNKNOWN", 5)
            );
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task RemoveStock_WithUnknownProduct_ThrowsException()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository = new MongoInventoryRepository(fixture.ConnectionString, "vendingmachine_inventory_test");

            // Act & Assert - Try to remove stock from a product that doesn't exist
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await repository.RemoveStockAsync("UNKNOWN", 5)
            );
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }
}
