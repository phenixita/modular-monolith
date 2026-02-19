using MediatR;
using Microsoft.Extensions.Logging;

namespace VendingMachine.Cash;

internal sealed class CashRegisterService : ICashRegisterService
{
    private readonly IMediator _mediator;
    private readonly ILogger<CashRegisterService> _logger;

    public CashRegisterService(IMediator mediator, ILogger<CashRegisterService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<decimal> GetBalance() =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.GetBalance",
            () => _mediator.Send(new GetBalanceQuery()));

    public Task Insert(decimal amount) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.Insert",
            () => _mediator.Send(new InsertCashCommand(amount)),
            parameters: new { Amount = amount });

    public Task Charge(decimal amount) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.Charge",
            () => _mediator.Send(new ChargeCashCommand(amount)),
            parameters: new { Amount = amount });

    public Task<decimal> RefundAll() =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.RefundAll",
            () => _mediator.Send(new RefundAllCommand()));
}
