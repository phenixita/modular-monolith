using Npgsql;

namespace VendingMachine.Cash;

public sealed class PostgresCashStorage : ICashStorage
{
    private readonly string _connectionString;

    public PostgresCashStorage(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public decimal GetBalance()
    {
        EnsureCreated();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM cash.cash_state WHERE property = 'balance';";
        var result = command.ExecuteScalar();

        return result is decimal value ? value : 0m;
    }

    public void SetBalance(decimal balance)
    {
        if (balance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(balance), "Balance cannot be negative.");
        }

        EnsureCreated();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO cash.cash_state (property, value)
            VALUES ('balance', @balance)
            ON CONFLICT (property) DO UPDATE
            SET value = EXCLUDED.value;
            """;
        command.Parameters.AddWithValue("balance", balance);
        command.ExecuteNonQuery();
    }

    public void EnsureCreated()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

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
        command.ExecuteNonQuery();
    }
}
