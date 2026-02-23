using System.Data;
using Npgsql;

namespace VendingMachine.Persistence;

public sealed class PostgresUnitOfWork : IUnitOfWork, ITransactionContext
{
    private static readonly AsyncLocal<TransactionContext?> _current = new();
    private readonly string _connectionString;

    public PostgresUnitOfWork(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public bool HasActiveTransaction => _current.Value is not null;

    public NpgsqlConnection? Connection => _current.Value?.Connection;

    public NpgsqlTransaction? Transaction => _current.Value?.Transaction;

    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return ExecuteAsync(async ct =>
        {
            await action(ct);
            return true;
        }, cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_current.Value is not null)
        {
            return await action(cancellationToken);
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        _current.Value = new TransactionContext(connection, transaction);
        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            _current.Value = null;
        }
    }

    private sealed record TransactionContext(NpgsqlConnection Connection, NpgsqlTransaction Transaction);
}
