using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using VendingMachine.Persistence.Abstractions;

namespace VendingMachine.Persistence;

public static class DI
{
    public static IServiceCollection AddPostgresPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<PostgresTransactionAccessor>();
        services.AddSingleton<IPostgresTransactionAccessor>(sp =>
            sp.GetRequiredService<PostgresTransactionAccessor>());

        services.AddSingleton<IDbConnectionFactory<NpgsqlConnection>>(_ =>
            new PostgresDbConnectionFactory(connectionString));
        services.AddSingleton<ITransactionManager<NpgsqlConnection, NpgsqlTransaction>, PostgresTransactionManager>();
        services.AddScoped<IUnitOfWork, PostgresUnitOfWork>();

        return services;
    }
}
