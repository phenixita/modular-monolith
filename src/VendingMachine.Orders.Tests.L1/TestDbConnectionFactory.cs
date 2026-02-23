using System.Data;
using Npgsql;
using VendingMachine.Persistence;

namespace VendingMachine.Orders.Tests.L1;

/// <summary>
/// Test implementation of the database connection factory.
/// Used for testing purposes with a direct connection string.
/// </summary>
public sealed class TestDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public TestDbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
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
