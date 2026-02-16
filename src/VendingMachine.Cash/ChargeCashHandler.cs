using MediatR;

namespace VendingMachine.Cash;

internal sealed class ChargeCashHandler(ICashStorage storage) : IRequestHandler<ChargeCashCommand, Unit>
{
    public Task<Unit> Handle(ChargeCashCommand request, CancellationToken cancellationToken)
    {
        ValidateAmountIsPositive(request.Amount);
        var balance = storage.GetBalance();
        EnsureBalanceIsSufficient(balance, request.Amount);
        DeductAmountFromBalance(balance, request.Amount);
        return Task.FromResult(Unit.Value);
    }

    private static void ValidateAmountIsPositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }
    }

    private static void EnsureBalanceIsSufficient(decimal balance, decimal amount)
    {
        if (balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }
    }

    private void DeductAmountFromBalance(decimal balance, decimal amount) =>
        storage.SetBalance(balance - amount);
}
