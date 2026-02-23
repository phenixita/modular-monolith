using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Xunit;

namespace VendingMachine.Inventory.Tests.L1;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private const int PostgresPort = 5432;

    private readonly IContainer _postgresContainer = new ContainerBuilder()
        .WithImage("postgres:16-alpine")
        .WithName($"vendingmachine-inventory-postgres-{Guid.NewGuid():N}")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "vendingmachine_inventory")
        .WithPortBinding(PostgresPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("database system is ready to accept connections"))
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        ConnectionString =
            $"Host={_postgresContainer.Hostname};Port={_postgresContainer.GetMappedPublicPort(PostgresPort)};Username=postgres;Password=postgres;Database=vendingmachine_inventory;SSL Mode=Disable;Trust Server Certificate=true;Pooling=false";
        await WaitUntilDatabaseReadyAsync();
    }

    public async Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE SCHEMA IF NOT EXISTS inventory;

            CREATE TABLE IF NOT EXISTS inventory.inventory_items (
                code VARCHAR(50) PRIMARY KEY,
                name TEXT NOT NULL,
                price NUMERIC(10,2) NOT NULL,
                quantity INTEGER NOT NULL DEFAULT 0 CHECK (quantity >= 0)
            );

            TRUNCATE TABLE inventory.inventory_items;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    private async Task WaitUntilDatabaseReadyAsync()
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();
                return;
            }
            catch (NpgsqlException)
            {
                await Task.Delay(250);
            }
        }

        throw new TimeoutException("PostgreSQL container did not become ready in time.");
    }
}
