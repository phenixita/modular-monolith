using Npgsql;
using VendingMachine.Persistence.Abstractions;

namespace VendingMachine.Persistence;

public sealed class PostgresTransactionManager(
    IDbConnectionFactory<NpgsqlConnection> connectionFactory)
    : ITransactionManager<NpgsqlConnection, NpgsqlTransaction>
{
    public async Task<ITransactionHandle<NpgsqlConnection, NpgsqlTransaction>> BeginAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        return new PostgresTransactionHandle(connection, transaction);
    }

    private sealed class PostgresTransactionHandle(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
        : ITransactionHandle<NpgsqlConnection, NpgsqlTransaction>
    {
        public NpgsqlConnection Connection => connection;

        public NpgsqlTransaction Transaction => transaction;

        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            transaction.RollbackAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
