namespace VendingMachine.Reporting.Abstractions;

public interface IReportingRepository
{
    Task RecordOrderAsync(
        string productCode,
        decimal price,
        DateTimeOffset orderedAt,
        CancellationToken cancellationToken = default);

    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
}
