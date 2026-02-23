using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Persistence;
using Xunit;

namespace VendingMachine.Cash.Tests.L1;

[Trait("Level", "L1")]
public sealed class CashRegisterInfrastructureTests : IClassFixture<InfrastructureFixture>, IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public CashRegisterInfrastructureTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetStateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CashRegister_PersistsBalanceAcrossInstances_WithPostgresStorage()
    {
        var storage1 = new PostgresCashStorage(_fixture.ConnectionString, new NullTransactionContext());
        await storage1.EnsureCreatedAsync();
        await storage1.SetBalanceAsync(0m);

        var register1 = BuildRegister(storage1);
        await register1.Insert(2.00m);
        await register1.Charge(1.25m);

        Assert.Equal(0.75m, await register1.GetBalance());

        var register2 = BuildRegister(new PostgresCashStorage(_fixture.ConnectionString, new NullTransactionContext()));
        Assert.Equal(0.75m, await register2.GetBalance());

        var refunded = await register2.RefundAll();
        Assert.Equal(0.75m, refunded);

        var register3 = BuildRegister(new PostgresCashStorage(_fixture.ConnectionString, new NullTransactionContext()));
        Assert.Equal(0m, await register3.GetBalance());
    }

    private static CashRegisterService BuildRegister(ICashStorage storage)
    {
        var services = new ServiceCollection();
        services.AddSingleton(storage);
        services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
        services.AddLogging();
        services.AddMediatR(typeof(CashRegisterService).Assembly);
        services.AddSingleton<CashRegisterService>();
        return services.BuildServiceProvider().GetRequiredService<CashRegisterService>();
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
            action(cancellationToken);

        public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
            action(cancellationToken);
    }

    private sealed class NullTransactionContext : VendingMachine.Persistence.ITransactionContext
    {
        public bool HasActiveTransaction => false;

        public Npgsql.NpgsqlConnection? Connection => null;

        public Npgsql.NpgsqlTransaction? Transaction => null;
    }
}
