namespace VendingMachine.Cash
{
    public interface ICashRegisterService
    {
        Task<decimal> GetBalance();

        Task Charge(decimal amount);

        Task Insert(decimal amount);

        Task<decimal> RefundAll();
    }
}