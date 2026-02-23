using System.Data.Common;

namespace VendingMachine.Persistence;

public interface IDbConnectionFactory<TConnection>
    where TConnection : DbConnection
{
    Task<TConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
