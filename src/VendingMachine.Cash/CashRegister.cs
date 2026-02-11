namespace VendingMachine.Cash;

public sealed class CashRegister : ICashRegister
{
    private readonly ICashStorage _storage;

    public CashRegister(ICashStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _storage.EnsureCreated();
    }

    public decimal Balance => _storage.GetBalance();

    public void Insert(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        var updatedBalance = _storage.GetBalance() + amount;
        _storage.SetBalance(updatedBalance);
    }

    public void Charge(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        var balance = _storage.GetBalance();
        if (balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        _storage.SetBalance(balance - amount);
    }

    public decimal RefundAll()
    {
        var refund = _storage.GetBalance();
        _storage.SetBalance(0m);
        return refund;
    }
}
