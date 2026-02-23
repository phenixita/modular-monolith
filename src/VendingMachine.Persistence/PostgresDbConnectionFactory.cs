using Npgsql;
using VendingMachine.Persistence.Abstractions;

namespace VendingMachine.Persistence;

public sealed class PostgresDbConnectionFactory(string connectionString)
    : IDbConnectionFactory<NpgsqlConnection>
{
    public async Task<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
