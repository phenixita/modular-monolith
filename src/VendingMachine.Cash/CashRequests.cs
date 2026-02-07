using MediatR;

namespace VendingMachine.Cash;

public sealed record InsertCashCommand(decimal Amount) : IRequest<Unit>;

public sealed class InsertCashHandler(CashRegister cashRegister) : IRequestHandler<InsertCashCommand, Unit>
{
    public Task<Unit> Handle(InsertCashCommand request, CancellationToken cancellationToken)
    {
        cashRegister.Insert(request.Amount);
        return Task.FromResult(Unit.Value);
    }
}

public sealed record ChargeCashCommand(decimal Amount) : IRequest<Unit>;

public sealed class ChargeCashHandler(CashRegister cashRegister) : IRequestHandler<ChargeCashCommand, Unit>
{
    public Task<Unit> Handle(ChargeCashCommand request, CancellationToken cancellationToken)
    {
        cashRegister.Charge(request.Amount);
        return Task.FromResult(Unit.Value);
    }
}

public sealed record GetBalanceQuery() : IRequest<decimal>;

public sealed class GetBalanceHandler(CashRegister cashRegister)
    : IRequestHandler<GetBalanceQuery, decimal>
{
    public Task<decimal> Handle(GetBalanceQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(cashRegister.Balance);
}

public sealed record RefundAllCommand() : IRequest<decimal>;

public sealed class RefundAllHandler(CashRegister cashRegister)
    : IRequestHandler<RefundAllCommand, decimal>
{
    public Task<decimal> Handle(RefundAllCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(cashRegister.RefundAll());
}
