using Npgsql;

namespace VendingMachine.Persistence;

public interface ITransactionContext
{
    bool HasActiveTransaction { get; }

    NpgsqlConnection? Connection { get; }

    NpgsqlTransaction? Transaction { get; }
}
