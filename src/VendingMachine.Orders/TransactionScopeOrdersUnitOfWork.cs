using System.Transactions;

namespace VendingMachine.Orders;

internal sealed class TransactionScopeOrdersUnitOfWork : IOrdersUnitOfWork
{
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.Serializable
            },
            TransactionScopeAsyncFlowOption.Enabled);

        var result = await action(cancellationToken);
        scope.Complete();
        return result;
    }
}
