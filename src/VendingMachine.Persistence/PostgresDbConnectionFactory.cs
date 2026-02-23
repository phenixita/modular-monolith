using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace VendingMachine.Persistence;

/// <summary>
/// PostgreSQL implementation of the database connection factory.
/// Uses IConfiguration to retrieve the connection string.
/// </summary>
public sealed class PostgresDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresDbConnectionFactory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration["postgres:connectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing configuration value 'postgres:connectionString'.");
        }

        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
