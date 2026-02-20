using MediatR;
using Microsoft.Extensions.Logging;
using VendingMachine.Orders.PlaceOrder;

namespace VendingMachine.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IMediator mediator, ILogger<OrderService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<OrderReceipt> PlaceOrder(string code, CancellationToken cancellationToken = default) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "OrderService.PlaceOrder",
            () => _mediator.Send(new PlaceOrderCommand(code), cancellationToken),
            successLevel: LogLevel.Information,
            parameters: new { Code = code });
}
