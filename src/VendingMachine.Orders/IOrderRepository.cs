namespace VendingMachine.Orders;

public interface IOrderRepository
{
    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
    Task AddAsync(OrderRecord order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderRecord>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}
