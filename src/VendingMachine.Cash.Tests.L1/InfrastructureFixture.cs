using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Xunit;

namespace VendingMachine.Cash.Tests.L1;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private const int PostgresPort = 5432;

    private readonly IContainer _postgresContainer = new ContainerBuilder()
        .WithImage("postgres:16-alpine")
        .WithName($"vendingmachine-cash-postgres-{Guid.NewGuid():N}")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "vendingmachine_cash")
        .WithPortBinding(PostgresPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("database system is ready to accept connections"))
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        ConnectionString =
            $"Host={_postgresContainer.Hostname};Port={_postgresContainer.GetMappedPublicPort(PostgresPort)};Username=postgres;Password=postgres;Database=vendingmachine_cash;SSL Mode=Disable;Trust Server Certificate=true;Pooling=false";
        await WaitUntilDatabaseReadyAsync();
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
