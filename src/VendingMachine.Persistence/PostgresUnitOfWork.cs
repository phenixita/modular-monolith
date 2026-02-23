using Npgsql;
using VendingMachine.Persistence.Abstractions;

namespace VendingMachine.Persistence;

public sealed class PostgresUnitOfWork(
    ITransactionManager<NpgsqlConnection, NpgsqlTransaction> transactionManager,
    PostgresTransactionAccessor transactionAccessor) : IUnitOfWork
{
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

        if (transactionAccessor.HasActiveTransaction)
        {
            return await action(cancellationToken);
        }

        await using var handle = await transactionManager.BeginAsync(cancellationToken);
        using var scope = transactionAccessor.Push(handle.Connection, handle.Transaction);

        try
        {
            var result = await action(cancellationToken);
            await handle.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await handle.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
