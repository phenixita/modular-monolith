using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Persistence;

public static class DI
{
    /// <summary>
    /// Registers persistence services (connection factory and transaction manager).
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, PostgresDbConnectionFactory>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        return services;
    }
}
