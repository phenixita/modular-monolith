using Npgsql;
using VendingMachine.Persistence;

namespace VendingMachine.Cash;

public sealed class PostgresCashRepository : ICashRepository
{
    private readonly string _connectionString;
    private readonly ITransactionContext _transactionAccessor;
    private static readonly ITransactionContext NoTransactionAccessor = new NullTransactionContext();

    public PostgresCashRepository(string connectionString, ITransactionContext? transactionAccessor = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
        _transactionAccessor = transactionAccessor ?? NoTransactionAccessor;
    }

    public async Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCreatedAsync(cancellationToken);

        await using var ownedConnection = await GetOwnedConnectionIfNeeded(cancellationToken);
        var connection = _transactionAccessor.Connection ?? ownedConnection!;

        await using var command = connection.CreateCommand();
        command.Transaction = _transactionAccessor.Transaction;
        command.CommandText = "SELECT value FROM cash.cash_state WHERE property = 'balance';";
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is decimal value ? value : 0m;
    }

    public async Task SetBalanceAsync(decimal balance, CancellationToken cancellationToken = default)
    {
        if (balance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(balance), "Balance cannot be negative.");
        }

        await EnsureCreatedAsync(cancellationToken);

        await using var ownedConnection = await GetOwnedConnectionIfNeeded(cancellationToken);
        var connection = _transactionAccessor.Connection ?? ownedConnection!;

        await using var command = connection.CreateCommand();
        command.Transaction = _transactionAccessor.Transaction;
        command.CommandText =
            """
            INSERT INTO cash.cash_state (property, value)
            VALUES ('balance', @balance)
            ON CONFLICT (property) DO UPDATE
            SET value = EXCLUDED.value;
            """;
        command.Parameters.AddWithValue("balance", balance);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var ownedConnection = await GetOwnedConnectionIfNeeded(cancellationToken);
        var connection = _transactionAccessor.Connection ?? ownedConnection!;

        await using var command = connection.CreateCommand();
        command.Transaction = _transactionAccessor.Transaction;
        command.CommandText =
            """
            CREATE SCHEMA IF NOT EXISTS cash;
            
            CREATE TABLE IF NOT EXISTS cash.cash_state (
                property VARCHAR(50) PRIMARY KEY,
                value NUMERIC(10,2) NOT NULL
            );

            INSERT INTO cash.cash_state (property, value)
            VALUES ('balance', 0.00)
            ON CONFLICT (property) DO NOTHING;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<NpgsqlConnection?> GetOwnedConnectionIfNeeded(CancellationToken cancellationToken)
    {
        if (_transactionAccessor.HasActiveTransaction)
        {
            return null;
        }

        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private sealed class NullTransactionContext : ITransactionContext
    {
        public bool HasActiveTransaction => false;

        public NpgsqlConnection? Connection => null;

        public NpgsqlTransaction? Transaction => null;
    }
}
