using MediatR;
using Microsoft.Extensions.Logging;
using VendingMachine.Orders;
using VendingMachine.Reporting.Abstractions;

namespace VendingMachine.Reporting;

internal sealed class OrderConfirmedHandler(
    IReportingRepository repository,
    ILogger<OrderConfirmedHandler> logger) : INotificationHandler<OrderConfirmed>
{
    private readonly IReportingRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILogger<OrderConfirmedHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task Handle(OrderConfirmed notification, CancellationToken cancellationToken) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "Reporting.OrderConfirmed",
            () => _repository.RecordOrderAsync(notification.ProductCode, notification.Price, notification.OrderedAt, cancellationToken),
            parameters: new
            {
                notification.ProductCode,
                notification.Price,
                notification.OrderedAt
            });
}
