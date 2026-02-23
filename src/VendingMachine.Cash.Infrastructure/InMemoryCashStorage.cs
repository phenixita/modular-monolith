namespace VendingMachine.Cash;

public sealed class InMemoryCashStorage : ICashStorage
{
    private decimal _balance;

    public decimal GetBalance() => _balance;

    public void SetBalance(decimal balance)
    {
        if (balance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(balance), "Balance cannot be negative.");
        }

        _balance = balance;
    }

    public void EnsureCreated()
    {
        // No-op for in-memory storage.
    }
}
