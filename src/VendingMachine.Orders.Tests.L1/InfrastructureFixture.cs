using MediatR;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private const int MongoPort = 27017;
    private const int PostgresPort = 5432;

    private readonly IContainer _mongoContainer = new ContainerBuilder()
        .WithImage("mongo:7.0")
        .WithName($"vendingmachine-mongo-{Guid.NewGuid():N}")
        .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "root")
        .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "root")
        .WithPortBinding(MongoPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Waiting for connections"))
        .Build();

    private readonly IContainer _postgresContainer = new ContainerBuilder()
        .WithImage("postgres:16-alpine")
        .WithName($"vendingmachine-postgres-{Guid.NewGuid():N}")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "vendingmachine")
        .WithPortBinding(PostgresPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("database system is ready to accept connections"))
        .Build();

    private string _mongoConnection = string.Empty;
    private string _mongoDatabase = $"vendingmachine_tests_{Guid.NewGuid():N}";
    private string _postgresConnection = string.Empty;
    private IServiceProvider? _provider;
    private IDisposable? _providerDisposable;

    public IMediator Mediator { get; private set; } = default!;
    public IInventoryRepository InventoryRepository { get; private set; } = default!;
    public IOrderRepository OrderRepository { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        await _postgresContainer.StartAsync();

        _mongoConnection =
            $"mongodb://root:root@{_mongoContainer.Hostname}:{_mongoContainer.GetMappedPublicPort(MongoPort)}/?authSource=admin";

        _postgresConnection =
            $"Host={_postgresContainer.Hostname};Port={_postgresContainer.GetMappedPublicPort(PostgresPort)};Username=postgres;Password=postgres;Database=vendingmachine;SSL Mode=Disable;Trust Server Certificate=true";

        await EnsureMongoAsync();

        InventoryRepository = new MongoInventoryRepository(_mongoConnection, _mongoDatabase);
        OrderRepository = new PostgresOrderRepository(_postgresConnection);
        await OrderRepository.EnsureCreatedAsync();

        var services = new ServiceCollection();
        services.AddSingleton<ICashStorage, InMemoryCashStorage>();
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
        await DropMongoAsync();
        await _postgresContainer.DisposeAsync();
        await _mongoContainer.DisposeAsync();
    }

    private async Task EnsureMongoAsync()
    {
        var client = new MongoClient(_mongoConnection);
        var database = client.GetDatabase(_mongoDatabase);
        await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
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
