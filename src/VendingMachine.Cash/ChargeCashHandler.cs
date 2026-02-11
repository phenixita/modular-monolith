using MediatR;

namespace VendingMachine.Cash;

public sealed class ChargeCashHandler(ICashStorage storage) : IRequestHandler<ChargeCashCommand, Unit>
{
    public Task<Unit> Handle(ChargeCashCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Amount), "Amount must be positive.");
        }

        var balance = storage.GetBalance();
        if (balance < request.Amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        storage.SetBalance(balance - request.Amount);
        return Task.FromResult(Unit.Value);
    }
}
