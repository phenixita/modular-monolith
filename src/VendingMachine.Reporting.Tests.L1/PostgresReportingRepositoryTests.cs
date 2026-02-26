using VendingMachine.Persistence;
using VendingMachine.Reporting.Infrastructure;
using Xunit;

namespace VendingMachine.Reporting.Tests.L1;

[Trait("Level", "L1")]
public sealed class PostgresReportingRepositoryTests : IClassFixture<InfrastructureFixture>, IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public PostgresReportingRepositoryTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetStateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDashboardStats_WhenNoOrders_ReturnsZeroes()
    {
        var repository = BuildRepository();

        var stats = await repository.GetDashboardStatsAsync();

        Assert.Equal(0m, stats.TotalRevenue);
        Assert.Equal(0, stats.OrderCount);
        Assert.Equal(0m, stats.AverageOrderValue);
    }

    [Fact]
    public async Task RecordOrder_ThenGetDashboardStats_ReturnsTotalsAndAverageComputedBySql()
    {
        var repository = BuildRepository();

        await repository.RecordOrderAsync("COLA", 1.50m, new DateTimeOffset(2026, 2, 25, 11, 0, 0, TimeSpan.Zero));
        await repository.RecordOrderAsync("WATER", 2.00m, new DateTimeOffset(2026, 2, 25, 11, 1, 0, TimeSpan.Zero));
        await repository.RecordOrderAsync("TEA", 2.50m, new DateTimeOffset(2026, 2, 25, 11, 2, 0, TimeSpan.Zero));

        var stats = await repository.GetDashboardStatsAsync();

        Assert.Equal(6.00m, stats.TotalRevenue);
        Assert.Equal(3, stats.OrderCount);
        Assert.Equal(2.00m, stats.AverageOrderValue);
    }

    private PostgresReportingRepository BuildRepository() =>
        new(_fixture.ConnectionString, new NullTransactionContext());

    private sealed class NullTransactionContext : ITransactionContext
    {
        public bool HasActiveTransaction => false;

        public Npgsql.NpgsqlConnection? Connection => null;

        public Npgsql.NpgsqlTransaction? Transaction => null;
    }
}
