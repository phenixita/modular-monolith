using MediatR;

namespace VendingMachine.Cash;

public sealed class InsertCashHandler(ICashStorage storage) : IRequestHandler<InsertCashCommand, Unit>
{
    public Task<Unit> Handle(InsertCashCommand request, CancellationToken cancellationToken)
    {
        ValidateAmountIsPositive(request.Amount);
        var updatedBalance = CalculateUpdatedBalance(request.Amount);
        PersistUpdatedBalance(updatedBalance);
        return Task.FromResult(Unit.Value);
    }

    private static void ValidateAmountIsPositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }
    }

    private decimal CalculateUpdatedBalance(decimal amount) =>
        storage.GetBalance() + amount;

    private void PersistUpdatedBalance(decimal balance) =>
        storage.SetBalance(balance);
}
