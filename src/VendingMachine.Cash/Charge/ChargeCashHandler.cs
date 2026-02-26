using MediatR;

namespace VendingMachine.Cash.Charge;

internal sealed class ChargeCashHandler(ICashRepository repository) : IRequestHandler<ChargeCashCommand, Unit>
{
    public async Task<Unit> Handle(ChargeCashCommand request, CancellationToken cancellationToken)
    {
        ValidateAmountIsPositive(request.Amount);
        var balance = await repository.GetBalanceAsync(cancellationToken);
        EnsureBalanceIsSufficient(balance, request.Amount);
        await DeductAmountFromBalance(balance, request.Amount, cancellationToken);
        return Unit.Value;
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

    private Task DeductAmountFromBalance(decimal balance, decimal amount, CancellationToken cancellationToken) =>
        repository.SetBalanceAsync(balance - amount, cancellationToken);
}
