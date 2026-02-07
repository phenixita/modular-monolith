namespace VendingMachine.Cash;

public sealed class CashRegister
{
    private decimal _balance;

    public decimal Balance => _balance;

    public void Insert(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        _balance += amount;
    }

    public void Charge(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        if (_balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        _balance -= amount;
    }

    public decimal RefundAll()
    {
        var refund = _balance;
        _balance = 0;
        return refund;
    }
}
