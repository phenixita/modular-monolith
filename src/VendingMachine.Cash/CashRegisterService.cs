using MediatR;
using Microsoft.Extensions.Logging;
using VendingMachine.Cash.Charge;
using VendingMachine.Cash.GetBalance;
using VendingMachine.Cash.InsertCache;
using VendingMachine.Cash.RefundAll;
using VendingMachine.Persistence;

namespace VendingMachine.Cash;

public sealed class CashRegisterService : ICashRegisterService
{
    private readonly IMediator _mediator;
    private readonly ILogger<CashRegisterService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CashRegisterService(
        IMediator mediator,
        ILogger<CashRegisterService> logger,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public Task<decimal> GetBalance(CancellationToken cancellationToken = default) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.GetBalance",
            () => _unitOfWork.ExecuteAsync(
                ct => _mediator.Send(new GetBalanceQuery(), ct),
                cancellationToken));

    public Task Insert(decimal amount, CancellationToken cancellationToken = default) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.Insert",
            () => _unitOfWork.ExecuteAsync(
                ct => _mediator.Send(new InsertCashCommand(amount), ct),
                cancellationToken),
            parameters: new { Amount = amount });

    public Task Charge(decimal amount, CancellationToken cancellationToken = default) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.Charge",
            () => _unitOfWork.ExecuteAsync(
                ct => _mediator.Send(new ChargeCashCommand(amount), ct),
                cancellationToken),
            parameters: new { Amount = amount });

    public Task<decimal> RefundAll(CancellationToken cancellationToken = default) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "CashRegisterService.RefundAll",
            () => _unitOfWork.ExecuteAsync(
                ct => _mediator.Send(new RefundAllCommand(), ct),
                cancellationToken));
}
