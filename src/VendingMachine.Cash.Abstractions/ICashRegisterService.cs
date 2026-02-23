namespace VendingMachine.Cash
{
    public interface ICashRegisterService
    {
        Task<decimal> GetBalance(CancellationToken cancellationToken = default);

        Task Charge(decimal amount, CancellationToken cancellationToken = default);

        Task Insert(decimal amount, CancellationToken cancellationToken = default);

        Task<decimal> RefundAll(CancellationToken cancellationToken = default);
    }
}
