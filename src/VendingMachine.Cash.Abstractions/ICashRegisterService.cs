namespace VendingMachine.Cash
{
    public interface ICashRegisterService
    {
        decimal Balance { get; }

        Task Charge(decimal amount);

        Task Insert(decimal amount);

        Task<decimal> RefundAll();
    }
}