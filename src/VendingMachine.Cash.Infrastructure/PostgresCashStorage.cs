using System.Data;
using VendingMachine.Persistence;

namespace VendingMachine.Cash;

public sealed class PostgresCashStorage : ICashStorage
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresCashStorage(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCreatedAsync(cancellationToken);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM cash.cash_state WHERE property = 'balance';";
        var result = await ExecuteScalarAsync(command, cancellationToken);

        return result is decimal value ? value : 0m;
    }

    public async Task SetBalanceAsync(decimal balance, CancellationToken cancellationToken = default)
    {
        if (balance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(balance), "Balance cannot be negative.");
        }

        await EnsureCreatedAsync(cancellationToken);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO cash.cash_state (property, value)
            VALUES ('balance', @balance)
            ON CONFLICT (property) DO UPDATE
            SET value = EXCLUDED.value;
            """;
        
        var parameter = command.CreateParameter();
        parameter.ParameterName = "balance";
        parameter.Value = balance;
        command.Parameters.Add(parameter);
        
        await ExecuteNonQueryAsync(command, cancellationToken);
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        using var command = connection.CreateCommand();
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
        await ExecuteNonQueryAsync(command, cancellationToken);
    }

    private static Task<object?> ExecuteScalarAsync(IDbCommand command, CancellationToken cancellationToken)
    {
        return Task.Run(() => command.ExecuteScalar(), cancellationToken);
    }

    private static Task<int> ExecuteNonQueryAsync(IDbCommand command, CancellationToken cancellationToken)
    {
        return Task.Run(() => command.ExecuteNonQuery(), cancellationToken);
    }
}
