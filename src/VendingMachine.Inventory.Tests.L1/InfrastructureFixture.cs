using DotNet.Testcontainers.Builders;
using Testcontainers.MongoDb;
using Xunit;

namespace VendingMachine.Inventory.Tests.L1;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:7")
        .WithName($"vendingmachine-inventory-mongo-{Guid.NewGuid():N}")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        ConnectionString = _mongoContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }
}
