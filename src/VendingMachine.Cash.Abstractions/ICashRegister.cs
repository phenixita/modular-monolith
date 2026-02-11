namespace VendingMachine.Cash
{
    public interface ICashRegister
    {
        decimal Balance { get; }

        void Charge(decimal amount);
        void Insert(decimal amount);
        decimal RefundAll();
    }
}