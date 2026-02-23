using System.Data.Common;

namespace VendingMachine.Persistence;

public interface ITransactionManager<TConnection, TTransaction>
    where TConnection : DbConnection
    where TTransaction : DbTransaction
{
    Task<ITransactionHandle<TConnection, TTransaction>> BeginAsync(CancellationToken cancellationToken = default);
}

public interface ITransactionHandle<TConnection, TTransaction> : IAsyncDisposable
    where TConnection : DbConnection
    where TTransaction : DbTransaction
{
    TConnection Connection { get; }

    TTransaction Transaction { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
