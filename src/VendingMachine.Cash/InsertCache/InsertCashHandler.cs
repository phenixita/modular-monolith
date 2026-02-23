using MediatR;

namespace VendingMachine.Cash.InsertCache;

internal sealed class InsertCashHandler(ICashStorage storage) : IRequestHandler<InsertCashCommand, Unit>
{
    public async Task<Unit> Handle(InsertCashCommand request, CancellationToken cancellationToken)
    {
        ValidateAmountIsPositive(request.Amount);
        var updatedBalance = await CalculateUpdatedBalanceAsync(request.Amount, cancellationToken);
        await PersistUpdatedBalanceAsync(updatedBalance, cancellationToken);
        return Unit.Value;
    }

    private static void ValidateAmountIsPositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }
    }

    private async Task<decimal> CalculateUpdatedBalanceAsync(decimal amount, CancellationToken cancellationToken)
    {
        var currentBalance = await storage.GetBalanceAsync(cancellationToken);
        return currentBalance + amount;
    }

    private Task PersistUpdatedBalanceAsync(decimal balance, CancellationToken cancellationToken)
    {
        return storage.SetBalanceAsync(balance, cancellationToken);
    }
}
