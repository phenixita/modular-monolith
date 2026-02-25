using Microsoft.Extensions.Logging;
using VendingMachine.Reporting.Abstractions;

namespace VendingMachine.Reporting
{
    public sealed class ReportingService(
        IReportingRepository repository,
        ILogger<ReportingService> logger) : IReportingService
    {
        private readonly IReportingRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILogger<ReportingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default) =>
            LoggingHelper.ExecuteWithLoggingAsync(
                _logger,
                "ReportingService.GetDashboardStats",
                () => _repository.GetDashboardStatsAsync(cancellationToken));
    }
}
