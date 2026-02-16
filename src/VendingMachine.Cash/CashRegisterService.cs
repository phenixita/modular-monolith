using MediatR;

namespace VendingMachine.Cash;

internal sealed class CashRegisterService : ICashRegisterService
{
    private readonly IMediator _mediator;

    public CashRegisterService(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public decimal Balance => _mediator.Send(new GetBalanceQuery()).GetAwaiter().GetResult();

    public async Task Insert(decimal amount) =>
        await _mediator.Send(new InsertCashCommand(amount));

    public async Task Charge(decimal amount) =>
        await _mediator.Send(new ChargeCashCommand(amount));

    public async Task<decimal> RefundAll() =>
        await _mediator.Send(new RefundAllCommand());
}
