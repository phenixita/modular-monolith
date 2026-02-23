using Npgsql;

namespace VendingMachine.Persistence;

public sealed class PostgresTransactionAccessor : IPostgresTransactionAccessor
{
    private static readonly AsyncLocal<StateHolder?> Current = new();

    public bool HasActiveTransaction => Current.Value?.State is not null;

    public NpgsqlConnection? Connection => Current.Value?.State?.Connection;

    public NpgsqlTransaction? Transaction => Current.Value?.State?.Transaction;

    public IDisposable Push(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        var parent = Current.Value?.State;
        Current.Value = new StateHolder(new PostgresTransactionState(connection, transaction, parent));
        return new ScopeHandle(this, parent);
    }

    private void Restore(PostgresTransactionState? parent)
    {
        Current.Value = parent is null ? null : new StateHolder(parent);
    }

    private sealed record StateHolder(PostgresTransactionState State);

    private sealed record PostgresTransactionState(
        NpgsqlConnection Connection,
        NpgsqlTransaction Transaction,
        PostgresTransactionState? Parent);

    private sealed class ScopeHandle(PostgresTransactionAccessor accessor, PostgresTransactionState? parent) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            accessor.Restore(parent);
        }
    }
}
