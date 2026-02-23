using Npgsql;

namespace VendingMachine.Inventory.Infrastructure;

public sealed class PostgresInventoryRepository : IInventoryRepository
{
    private readonly string _connectionString;

    public PostgresInventoryRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT code, name, price FROM inventory.inventory_items ORDER BY code;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var products = new List<Product>();
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new Product(reader.GetString(0), reader.GetString(1), reader.GetDecimal(2)));
        }

        return products;
    }

    public async Task<Product> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT code, name, price FROM inventory.inventory_items WHERE code = @code;";
        command.Parameters.AddWithValue("code", normalized);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        return new Product(reader.GetString(0), reader.GetString(1), reader.GetDecimal(2));
    }

    public async Task<int> GetQuantityAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT quantity FROM inventory.inventory_items WHERE code = @code;";
        command.Parameters.AddWithValue("code", normalized);
        var quantity = await command.ExecuteScalarAsync(cancellationToken);
        return quantity is int value ? value : 0;
    }

    public async Task AddOrUpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        var normalized = NormalizeCode(product.Code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO inventory.inventory_items (code, name, price, quantity)
            VALUES (@code, @name, @price, 0)
            ON CONFLICT (code) DO UPDATE
            SET name = EXCLUDED.name, price = EXCLUDED.price;
            """;
        command.Parameters.AddWithValue("code", normalized);
        command.Parameters.AddWithValue("name", product.Name);
        command.Parameters.AddWithValue("price", product.Price);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM inventory.inventory_items WHERE code = @code;";
        command.Parameters.AddWithValue("code", normalized);
        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affected == 0)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }
    }

    public async Task AddStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var normalized = NormalizeCode(code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE inventory.inventory_items SET quantity = quantity + @quantity WHERE code = @code;";
        command.Parameters.AddWithValue("quantity", quantity);
        command.Parameters.AddWithValue("code", normalized);
        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affected == 0)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }
    }

    public async Task RemoveStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var normalized = NormalizeCode(code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var quantityCommand = connection.CreateCommand();
        quantityCommand.CommandText =
            "SELECT quantity FROM inventory.inventory_items WHERE code = @code;";
        quantityCommand.Parameters.AddWithValue("code", normalized);
        var currentQuantity = await quantityCommand.ExecuteScalarAsync(cancellationToken);

        if (currentQuantity is not int available)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }

        if (available < quantity)
        {
            throw new InvalidOperationException("Not enough stock.");
        }

        await using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE inventory.inventory_items SET quantity = quantity - @quantity WHERE code = @code;";
        command.Parameters.AddWithValue("quantity", quantity);
        command.Parameters.AddWithValue("code", normalized);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetStockAsync(string code, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be zero or positive.");
        }

        var normalized = NormalizeCode(code);
        await EnsureCreatedAsync(cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE inventory.inventory_items SET quantity = @quantity WHERE code = @code;";
        command.Parameters.AddWithValue("quantity", quantity);
        command.Parameters.AddWithValue("code", normalized);
        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affected == 0)
        {
            throw new KeyNotFoundException($"Unknown product code '{code}'.");
        }
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE SCHEMA IF NOT EXISTS inventory;

            CREATE TABLE IF NOT EXISTS inventory.inventory_items (
                code VARCHAR(50) PRIMARY KEY,
                name TEXT NOT NULL,
                price NUMERIC(10,2) NOT NULL,
                quantity INTEGER NOT NULL DEFAULT 0 CHECK (quantity >= 0)
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }
}
