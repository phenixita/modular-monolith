using VendingMachine.Persistence;
using Xunit;

namespace VendingMachine.Cash.Tests.L1;

[Trait("Level", "L1")]
public sealed class PostgresCashRepositoryTests : IClassFixture<InfrastructureFixture>, IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public PostgresCashRepositoryTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetStateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EnsureCreatedAsync_CreatesSchemaAndTable()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        await repository.EnsureCreatedAsync();

        // Assert: Should not throw and should create initial state
        var balance = await repository.GetBalanceAsync();
        Assert.Equal(0m, balance);
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsZero_WhenNoBalanceSet()
    {
        // Arrange
        var repository = CreateRepository();
        await repository.EnsureCreatedAsync();

        // Act
        var balance = await repository.GetBalanceAsync();

        // Assert
        Assert.Equal(0m, balance);
    }

    [Fact]
    public async Task SetBalanceAsync_PersistsBalance()
    {
        // Arrange
        var repository = CreateRepository();
        await repository.EnsureCreatedAsync();

        // Act
        await repository.SetBalanceAsync(42.50m);

        // Assert
        var balance = await repository.GetBalanceAsync();
        Assert.Equal(42.50m, balance);
    }

    [Fact]
    public async Task SetBalanceAsync_UpdatesExistingBalance()
    {
        // Arrange
        var repository = CreateRepository();
        await repository.EnsureCreatedAsync();
        await repository.SetBalanceAsync(10.00m);

        // Act
        await repository.SetBalanceAsync(25.75m);

        // Assert
        var balance = await repository.GetBalanceAsync();
        Assert.Equal(25.75m, balance);
    }

    [Fact]
    public async Task SetBalanceAsync_ThrowsArgumentOutOfRangeException_WhenBalanceIsNegative()
    {
        // Arrange
        var repository = CreateRepository();
        await repository.EnsureCreatedAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await repository.SetBalanceAsync(-1.00m));
    }

    

    private PostgresCashRepository CreateRepository() =>
        new(_fixture.ConnectionString, new NullTransactionContext());

    private sealed class NullTransactionContext : ITransactionContext
    {
        public bool HasActiveTransaction => false;

        public Npgsql.NpgsqlConnection? Connection => null;

        public Npgsql.NpgsqlTransaction? Transaction => null;
    }
}
