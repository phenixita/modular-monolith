using Npgsql;

namespace VendingMachine.Orders;

public sealed class PostgresOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public PostgresOrderRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS orders (
                id UUID PRIMARY KEY,
                product_code TEXT NOT NULL,
                product_name TEXT NOT NULL,
                price NUMERIC(10,2) NOT NULL,
                status INT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddAsync(OrderRecord order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO orders (id, product_code, product_name, price, status, created_at)
            VALUES (@id, @product_code, @product_name, @price, @status, @created_at);
            """;

        command.Parameters.AddWithValue("id", order.Id);
        command.Parameters.AddWithValue("product_code", order.ProductCode);
        command.Parameters.AddWithValue("product_name", order.ProductName);
        command.Parameters.AddWithValue("price", order.Price);
        command.Parameters.AddWithValue("status", (int)order.Status);
        command.Parameters.AddWithValue("created_at", order.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderRecord>> GetRecentAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be positive.");
        }

        var orders = new List<OrderRecord>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, product_code, product_name, price, status, created_at
            FROM orders
            ORDER BY created_at DESC
            LIMIT @limit;
            """;
        command.Parameters.AddWithValue("limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new OrderRecord(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDecimal(3),
                (OrderStatus)reader.GetInt32(4),
                reader.GetFieldValue<DateTimeOffset>(5)));
        }

        return orders;
    }
}
