using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using Xunit;

namespace VendingMachine.Inventory.Tests.L1;

[Trait("Level", "L1")]
public sealed class InventoryInfrastructureTests
{
    [Fact]
    public async Task AddStock_PersistsDataInPostgres()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new PostgresInventoryRepository(fixture.ConnectionString);
            var service1 = BuildInventoryService(repository1);

            // Create a product first
            var product = new Product("COLA", "Coca Cola", 1.50m);
            await service1.CreateProduct(product);

            // Act - Add stock using first repository instance
            await service1.AddStock("COLA", 5);

            // Assert - Verify persistence by reading from a new repository instance
            var repository2 = new PostgresInventoryRepository(fixture.ConnectionString);
            var quantity = await repository2.GetQuantityAsync("COLA");
            Assert.Equal(5, quantity);

            // Act - Add more stock
            var service2 = BuildInventoryService(repository2);
            await service2.AddStock("COLA", 3);

            // Assert - Verify the total is correct
            var repository3 = new PostgresInventoryRepository(fixture.ConnectionString);
            var finalQuantity = await repository3.GetQuantityAsync("COLA");
            Assert.Equal(8, finalQuantity);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task RemoveStock_PersistsDataInPostgres()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new PostgresInventoryRepository(fixture.ConnectionString);
            var service1 = BuildInventoryService(repository1);

            // Create a product and set initial stock
            var product = new Product("WATER", "Water", 1.00m);
            await service1.CreateProduct(product);
            await service1.SetStock("WATER", 20);

            // Act - Remove stock using first repository instance
            await service1.RemoveStock("WATER", 15);

            // Assert - Verify persistence by reading from a new repository instance
            var repository2 = new PostgresInventoryRepository(fixture.ConnectionString);
            var quantity = await repository2.GetQuantityAsync("WATER");
            Assert.Equal(5, quantity);

            // Act - Remove more stock
            var service2 = BuildInventoryService(repository2);
            await service2.RemoveStock("WATER", 5);

            // Assert - Verify the final quantity is zero
            var repository3 = new PostgresInventoryRepository(fixture.ConnectionString);
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
            var repository = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository);

            // Create multiple products
            await service.CreateProduct(new Product("COLA", "Coca Cola", 1.50m));
            await service.CreateProduct(new Product("WATER", "Water", 1.00m));
            await service.CreateProduct(new Product("JUICE", "Orange Juice", 2.00m));

            // Act - Add stock to each product
            await service.AddStock("COLA", 10);
            await service.AddStock("WATER", 15);
            await service.AddStock("JUICE", 8);

            // Assert - Verify each product has correct stock
            var repository2 = new PostgresInventoryRepository(fixture.ConnectionString);
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
            var repository = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository);

            await service.CreateProduct(new Product("COLA", "Coca Cola", 1.50m));
            await service.SetStock("COLA", 3);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.RemoveStock("COLA", 5)
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
            var repository = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository);

            // Act & Assert - Try to add stock to a product that doesn't exist
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await service.AddStock("UNKNOWN", 5)
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
            var repository = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await service.RemoveStock("UNKNOWN", 5)
            );
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreateProduct_PersistsInPostgres()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository1);
            var product = new Product("COLA", "Coca Cola", 1.50m);

            // Act
            await service.CreateProduct(product);

            // Assert
            var repository2 = new PostgresInventoryRepository(fixture.ConnectionString);
            var stored = await repository2.GetByCodeAsync("COLA");
            Assert.Equal(1.50m, stored.Price);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateProduct_PersistsPriceInPostgres()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository1);
            await service.CreateProduct(new Product("WATER", "Water", 1.00m));

            // Act
            await service.UpdateProduct(new Product("WATER", "Water", 1.25m));

            // Assert
            var repository2 = new PostgresInventoryRepository(fixture.ConnectionString);
            var stored = await repository2.GetByCodeAsync("WATER");
            Assert.Equal(1.25m, stored.Price);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task DeleteProduct_WithZeroStock_RemovesProductInPostgres()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository1 = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository1);
            await service.CreateProduct(new Product("JUICE", "Orange Juice", 2.00m));
            await service.SetStock("JUICE", 0);

            // Act
            await service.DeleteProduct("JUICE");

            // Assert
            var repository2 = new PostgresInventoryRepository(fixture.ConnectionString);
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await repository2.GetByCodeAsync("JUICE")
            );
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task DeleteProduct_WithStock_ThrowsException()
    {
        // Arrange
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var repository = new PostgresInventoryRepository(fixture.ConnectionString);
            var service = BuildInventoryService(repository);
            await service.CreateProduct(new Product("TEA", "Tea", 0.80m));
            await service.SetStock("TEA", 2);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.DeleteProduct("TEA")
            );
        }
        finally
        {
            await fixture.DisposeAsync();
        }
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
