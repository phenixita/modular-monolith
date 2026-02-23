using System.Data;

namespace VendingMachine.Persistence;

/// <summary>
/// Implementation of unit of work for managing database transactions.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private bool _disposed;

    public UnitOfWork(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public IDbTransaction Transaction => _transaction;

    public IDbConnection Connection => _connection;

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _transaction.Commit();
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _transaction.Rollback();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await Task.Run(() =>
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        });

        _disposed = true;
    }
}
