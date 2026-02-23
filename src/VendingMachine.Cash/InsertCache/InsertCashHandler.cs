using MediatR;

namespace VendingMachine.Cash.InsertCache;

internal sealed class InsertCashHandler(ICashStorage storage) : IRequestHandler<InsertCashCommand, Unit>
{
    public async Task<Unit> Handle(InsertCashCommand request, CancellationToken cancellationToken)
    {
        ValidateAmountIsPositive(request.Amount);
        var updatedBalance = await CalculateUpdatedBalance(request.Amount, cancellationToken);
        await PersistUpdatedBalance(updatedBalance, cancellationToken);
        return Unit.Value;
    }

    private static void ValidateAmountIsPositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }
    }

    private async Task<decimal> CalculateUpdatedBalance(decimal amount, CancellationToken cancellationToken) =>
        await storage.GetBalanceAsync(cancellationToken) + amount;

    private Task PersistUpdatedBalance(decimal balance, CancellationToken cancellationToken) =>
        storage.SetBalanceAsync(balance, cancellationToken);
}
