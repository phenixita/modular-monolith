namespace VendingMachine.Reporting.Abstractions;

public interface IReportingService
{
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
