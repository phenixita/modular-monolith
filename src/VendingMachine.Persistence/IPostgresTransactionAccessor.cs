using Npgsql;

namespace VendingMachine.Persistence;

public interface IPostgresTransactionAccessor
{
    bool HasActiveTransaction { get; }

    NpgsqlConnection? Connection { get; }

    NpgsqlTransaction? Transaction { get; }
}
