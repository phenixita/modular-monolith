namespace VendingMachine.Cash;

public interface ICashStorage
{
    Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);

    Task SetBalanceAsync(decimal balance, CancellationToken cancellationToken = default);

    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
}
