using Npgsql;
using VendingMachine.Reporting.Abstractions;
using VendingMachine.Persistence;

namespace VendingMachine.Reporting.Infrastructure
{
    public sealed class PostgresReportingRepository : IReportingRepository
    {
        private readonly string _connectionString;
        private readonly ITransactionContext _transactionContext;
        private static readonly ITransactionContext NoTransactionContext = new NullTransactionContext();

        public PostgresReportingRepository(string connectionString, ITransactionContext? transactionContext = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            }

            _connectionString = connectionString;
            _transactionContext = transactionContext ?? NoTransactionContext;
        }

        public async Task RecordOrderAsync(
            string productCode,
            decimal price,
            DateTimeOffset orderedAt,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productCode))
            {
                throw new ArgumentException("Product code is required.", nameof(productCode));
            }

            if (price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
            }

            await EnsureCreatedAsync(cancellationToken);

            await using var ownedConnection = await GetOwnedConnectionIfNeeded(cancellationToken);
            var connection = _transactionContext.Connection ?? ownedConnection!;

            await using var command = connection.CreateCommand();
            command.Transaction = _transactionContext.Transaction;
            command.CommandText =
                """
                INSERT INTO reporting.confirmed_orders (product_code, total_euro, ordered_at)
                VALUES (@productCode, @price, @orderedAt);
                """;
            command.Parameters.AddWithValue("productCode", productCode.Trim().ToUpperInvariant());
            command.Parameters.AddWithValue("price", price);
            command.Parameters.AddWithValue("orderedAt", orderedAt);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
        {
            await EnsureCreatedAsync(cancellationToken);

            await using var ownedConnection = await GetOwnedConnectionIfNeeded(cancellationToken);
            var connection = _transactionContext.Connection ?? ownedConnection!;

            await using var command = connection.CreateCommand();
            command.Transaction = _transactionContext.Transaction;
            command.CommandText =
                """
                SELECT
                    COALESCE(SUM(total_euro), 0.00),
                    COUNT(*),
                    COALESCE(AVG(total_euro), 0.00)
                FROM reporting.confirmed_orders;
                """;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return new DashboardStats(0m, 0, 0m);
            }

            var totalEuro = reader.GetDecimal(0);
            var totalOrders = reader.GetInt64(1);
            var averageEuro = reader.GetDecimal(2);

            return new DashboardStats(totalEuro, checked((int)totalOrders), averageEuro);
        }

        public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            await using var ownedConnection = await GetOwnedConnectionIfNeeded(cancellationToken);
            var connection = _transactionContext.Connection ?? ownedConnection!;

            await using var command = connection.CreateCommand();
            command.Transaction = _transactionContext.Transaction;
            command.CommandText =
                """
                CREATE SCHEMA IF NOT EXISTS reporting;

                CREATE TABLE IF NOT EXISTS reporting.confirmed_orders (
                    id BIGSERIAL PRIMARY KEY,
                    product_code VARCHAR(50) NOT NULL,
                    total_euro NUMERIC(10,2) NOT NULL,
                    ordered_at TIMESTAMPTZ NOT NULL
                );
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private async Task<NpgsqlConnection?> GetOwnedConnectionIfNeeded(CancellationToken cancellationToken)
        {
            if (_transactionContext.HasActiveTransaction)
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
}
