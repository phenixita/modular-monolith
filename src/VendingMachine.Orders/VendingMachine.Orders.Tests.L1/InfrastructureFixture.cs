using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private const string DefaultMongoConnection = "mongodb://root:root@localhost:27017/?authSource=admin";
    private const string DefaultPostgresConnection =
        "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

    private string _mongoConnection = DefaultMongoConnection;
    private string _mongoDatabase = $"vendingmachine_tests_{Guid.NewGuid():N}";
    private string _postgresAdminConnection = DefaultPostgresConnection;
    private string _postgresDatabase = $"vendingmachine_tests_{Guid.NewGuid():N}";
    private string _postgresConnection = string.Empty;
    private IServiceProvider? _provider;
    private IDisposable? _providerDisposable;

    public IMediator Mediator { get; private set; } = default!;
    public IInventoryRepository InventoryRepository { get; private set; } = default!;
    public IOrderRepository OrderRepository { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _mongoConnection = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? DefaultMongoConnection;

        var postgresConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(postgresConnection))
        {
            _postgresAdminConnection = postgresConnection;
        }

        var postgresBuilder = new NpgsqlConnectionStringBuilder(_postgresAdminConnection);
        if (string.IsNullOrWhiteSpace(postgresBuilder.Database))
        {
            postgresBuilder.Database = "postgres";
        }

        _postgresAdminConnection = postgresBuilder.ToString();
        _postgresConnection = new NpgsqlConnectionStringBuilder(postgresBuilder.ConnectionString)
        {
            Database = _postgresDatabase
        }.ToString();

        await EnsurePostgresAsync();
        await EnsureMongoAsync();

        InventoryRepository = new MongoInventoryRepository(_mongoConnection, _mongoDatabase);
        OrderRepository = new PostgresOrderRepository(_postgresConnection);
        await OrderRepository.EnsureCreatedAsync();

        var services = new ServiceCollection();
        services.AddSingleton<CashRegister>();
        services.AddSingleton<IInventoryRepository>(InventoryRepository);
        services.AddSingleton<IOrderRepository>(OrderRepository);
        services.AddMediatR(typeof(CashRegister).Assembly, typeof(InventoryCatalog).Assembly, typeof(OrderService).Assembly);

        _provider = services.BuildServiceProvider();
        _providerDisposable = _provider as IDisposable;
        Mediator = _provider.GetRequiredService<IMediator>();
    }

    public async Task DisposeAsync()
    {
        _providerDisposable?.Dispose();
        await DropPostgresAsync();
        await DropMongoAsync();
    }

    private async Task EnsurePostgresAsync()
    {
        await using var connection = new NpgsqlConnection(_postgresAdminConnection);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{_postgresDatabase}\";";
        await command.ExecuteNonQueryAsync();
    }

    private async Task EnsureMongoAsync()
    {
        var client = new MongoClient(_mongoConnection);
        var database = client.GetDatabase(_mongoDatabase);
        await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
    }

    private async Task DropPostgresAsync()
    {
        if (string.IsNullOrWhiteSpace(_postgresConnection))
        {
            return;
        }

        try
        {
            await using var connection = new NpgsqlConnection(_postgresAdminConnection);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS \"{_postgresDatabase}\" WITH (FORCE);";
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private async Task DropMongoAsync()
    {
        try
        {
            var client = new MongoClient(_mongoConnection);
            await client.DropDatabaseAsync(_mongoDatabase);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
