using System.Data;

namespace VendingMachine.Persistence;

/// <summary>
/// Represents a unit of work that manages database transactions.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets the current database transaction.
    /// </summary>
    IDbTransaction Transaction { get; }

    /// <summary>
    /// Gets the database connection associated with this unit of work.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
