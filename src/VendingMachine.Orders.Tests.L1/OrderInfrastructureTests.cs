using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Orders.PlaceOrder;
using VendingMachine.Persistence;
using Xunit;

namespace VendingMachine.Orders.Tests.L1;

[Trait("Level", "L1")]
public sealed class OrderInfrastructureTests : IClassFixture<InfrastructureFixture>, IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public OrderInfrastructureTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetStateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PlaceOrder_PersistsCashAndInventory()
    {
        var cashrepository = new PostgresCashRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        await cashrepository.EnsureCreatedAsync();
        await cashrepository.SetBalanceAsync(5.00m);

        var inventory = new PostgresInventoryRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        await inventory.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await inventory.SetStockAsync("COLA", 2);

        var services = BuildServices(_fixture.PostgresConnectionString, failOnRemoveStock: false);
        var orderService = services.GetRequiredService<IOrderService>();

        var receipt = await orderService.PlaceOrder("COLA");

        Assert.Equal("COLA", receipt.ProductCode);
        Assert.Equal(3.50m, receipt.Balance);
        Assert.Equal(1, receipt.Stock);

        var cashrepository2 = new PostgresCashRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        Assert.Equal(3.50m, await cashrepository2.GetBalanceAsync());

        var inventory2 = new PostgresInventoryRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        Assert.Equal(1, await inventory2.GetQuantityAsync("COLA"));
    }

    [Fact]
    public async Task PlaceOrder_WhenStockUpdateFails_RollsBackCashAndKeepsInventoryUnchanged()
    {
        var cashrepository = new PostgresCashRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        await cashrepository.EnsureCreatedAsync();
        await cashrepository.SetBalanceAsync(5.00m);

        var baseRepository = new PostgresInventoryRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        await baseRepository.AddOrUpdateAsync(new Product("COLA", "Coca Cola", 1.50m));
        await baseRepository.SetStockAsync("COLA", 2);
        var services = BuildServices(_fixture.PostgresConnectionString, failOnRemoveStock: true);
        var mediator = services.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await mediator.Send(new PlaceOrderCommand("COLA"))
        );

        var cashrepository2 = new PostgresCashRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        Assert.Equal(5.00m, await cashrepository2.GetBalanceAsync());

        var inventory2 = new PostgresInventoryRepository(
            _fixture.PostgresConnectionString,
            new NullTransactionContext());
        Assert.Equal(2, await inventory2.GetQuantityAsync("COLA"));
    }

    private static ServiceProvider BuildServices(string connectionString, bool failOnRemoveStock)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICashRepository>(sp =>
            new PostgresCashRepository(
                connectionString,
                sp.GetRequiredService<ITransactionContext>()));
        services.AddSingleton<IInventoryRepository>(sp =>
        {
            var repository = new PostgresInventoryRepository(
                connectionString,
                sp.GetRequiredService<ITransactionContext>());

            return failOnRemoveStock
                ? new FailingOnRemoveStockRepository(repository)
                : repository;
        });
        services.AddLogging();
        services.AddMediatR(
            typeof(OrderService).Assembly,
            typeof(CashRegisterService).Assembly,
            typeof(InventoryService).Assembly);
        services.AddPostgresPersistence(connectionString);
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<IInventoryService, InventoryService>();
        services.AddSingleton<ICashRegisterService, CashRegisterService>();
        return services.BuildServiceProvider();
    }

    private sealed class FailingOnRemoveStockRepository(IInventoryRepository innerRepository) : IInventoryRepository
    {
        public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
            innerRepository.GetAllAsync(cancellationToken);

        public Task<Product> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            innerRepository.GetByCodeAsync(code, cancellationToken);

        public Task<int> GetQuantityAsync(string code, CancellationToken cancellationToken = default) =>
            innerRepository.GetQuantityAsync(code, cancellationToken);

        public Task AddOrUpdateAsync(Product product, CancellationToken cancellationToken = default) =>
            innerRepository.AddOrUpdateAsync(product, cancellationToken);

        public Task DeleteAsync(string code, CancellationToken cancellationToken = default) =>
            innerRepository.DeleteAsync(code, cancellationToken);

        public Task AddStockAsync(string code, int quantity, CancellationToken cancellationToken = default) =>
            innerRepository.AddStockAsync(code, quantity, cancellationToken);

        public Task RemoveStockAsync(string code, int quantity, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated stock persistence failure.");

        public Task SetStockAsync(string code, int quantity, CancellationToken cancellationToken = default) =>
            innerRepository.SetStockAsync(code, quantity, cancellationToken);
    }

    private sealed class NullTransactionContext : ITransactionContext
    {
        public bool HasActiveTransaction => false;

        public Npgsql.NpgsqlConnection? Connection => null;

        public Npgsql.NpgsqlTransaction? Transaction => null;
    }
}
