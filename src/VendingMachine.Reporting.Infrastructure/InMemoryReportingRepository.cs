using VendingMachine.Reporting.Abstractions;

namespace VendingMachine.Reporting.Infrastructure;

public sealed class InMemoryReportingRepository : IReportingRepository
{
    private readonly List<OrderRecord> _orders = new();

    public Task RecordOrderAsync(
        string productCode,
        decimal price,
        DateTimeOffset orderedAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code is required.", nameof(productCode));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        _orders.Add(new OrderRecord(productCode, price, orderedAt));
        return Task.CompletedTask;
    }

    public Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        if (_orders.Count == 0)
        {
            return Task.FromResult(new DashboardStats(0m, 0, 0m));
        }

        var totalRevenue = _orders.Sum(o => o.Price);
        var orderCount = _orders.Count;
        var averageOrderValue = totalRevenue / orderCount;

        return Task.FromResult(new DashboardStats(totalRevenue, orderCount, averageOrderValue));
    }

    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        // No-op for in-memory repository.
        return Task.CompletedTask;
    }

    private sealed record OrderRecord(string ProductCode, decimal Price, DateTimeOffset OrderedAt);
}
