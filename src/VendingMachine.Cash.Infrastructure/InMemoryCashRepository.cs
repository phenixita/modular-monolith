namespace VendingMachine.Cash;

public sealed class InMemoryCashrepository : ICashRepository
{
    private decimal _balance;

    public Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_balance);

    public Task SetBalanceAsync(decimal balance, CancellationToken cancellationToken = default)
    {
        if (balance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(balance), "Balance cannot be negative.");
        }

        _balance = balance;
        return Task.CompletedTask;
    }

    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        // No-op for in-memory repository.
        return Task.CompletedTask;
    }
}
