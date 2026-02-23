using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<PostgresUnitOfWork>(sp => new PostgresUnitOfWork(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PostgresUnitOfWork>());
        services.AddScoped<ITransactionContext>(sp => sp.GetRequiredService<PostgresUnitOfWork>());

        return services;
    }
}
