using System.Data;

namespace VendingMachine.Persistence;

/// <summary>
/// Implementation of transaction manager with Serializable isolation level as default.
/// </summary>
public sealed class TransactionManager : ITransactionManager
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IsolationLevel _defaultIsolationLevel;

    public TransactionManager(IDbConnectionFactory connectionFactory)
        : this(connectionFactory, IsolationLevel.Serializable)
    {
    }

    public TransactionManager(IDbConnectionFactory connectionFactory, IsolationLevel isolationLevel)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _defaultIsolationLevel = isolationLevel;
    }

    public async Task<IUnitOfWork> BeginAsync(CancellationToken cancellationToken = default)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var transaction = connection.BeginTransaction(_defaultIsolationLevel);
        return new UnitOfWork(connection, transaction);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<IUnitOfWork, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var unitOfWork = await BeginAsync(cancellationToken);

        try
        {
            var result = await operation(unitOfWork, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<IUnitOfWork, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var unitOfWork = await BeginAsync(cancellationToken);

        try
        {
            await operation(unitOfWork, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
