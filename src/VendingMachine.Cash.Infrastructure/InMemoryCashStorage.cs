namespace VendingMachine.Cash;

public sealed class InMemoryCashStorage : ICashStorage
{
    private decimal _balance;

    public Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_balance);
    }

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
        // No-op for in-memory storage.
        return Task.CompletedTask;
    }
}
