using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.MongoDb;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private const int PostgresPort = 5432;

    private readonly IContainer _postgresContainer = new ContainerBuilder()
        .WithImage("postgres:16-alpine")
        .WithName($"vendingmachine-orders-postgres-{Guid.NewGuid():N}")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "vendingmachine_cash")
        .WithPortBinding(PostgresPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("database system is ready to accept connections"))
        .Build();

    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:7")
        .WithName($"vendingmachine-orders-mongo-{Guid.NewGuid():N}")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    public string PostgresConnectionString { get; private set; } = string.Empty;

    public string MongoConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        PostgresConnectionString =
            $"Host={_postgresContainer.Hostname};Port={_postgresContainer.GetMappedPublicPort(PostgresPort)};Username=postgres;Password=postgres;Database=vendingmachine_cash;SSL Mode=Disable;Trust Server Certificate=true";

        await _mongoContainer.StartAsync();
        MongoConnectionString = _mongoContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _mongoContainer.DisposeAsync();
    }
}
