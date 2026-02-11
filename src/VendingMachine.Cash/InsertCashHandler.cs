using MediatR;

namespace VendingMachine.Cash;

public sealed class InsertCashHandler(ICashStorage storage) : IRequestHandler<InsertCashCommand, Unit>
{
    public Task<Unit> Handle(InsertCashCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Amount), "Amount must be positive.");
        }

        var updatedBalance = storage.GetBalance() + request.Amount;
        storage.SetBalance(updatedBalance);
        return Task.FromResult(Unit.Value);
    }
}
