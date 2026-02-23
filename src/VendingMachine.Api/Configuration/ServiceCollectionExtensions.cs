using Microsoft.Extensions.Options;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;
using VendingMachine.Persistence;

namespace VendingMachine.Api.Configuration;

internal static class ServiceCollectionExtensions
{
    private const string MissingConnectionStringMessage = "Missing configuration value 'postgres.connectionString'.";

    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IValidateOptions<InfrastructureOptions>, InfrastructureOptionsValidator>();
        services
            .AddOptions<InfrastructureOptions>()
            .Bind(configuration.GetSection(InfrastructureOptions.SectionName))
            .ValidateOnStart();

        var connectionString = GetPostgresConnectionString(configuration);
        services.AddPostgresPersistence(connectionString);

        services.AddInventoryModule();

        services.AddCashRegisterModule();

        services.AddOrdersModule();

        return services;
    }

    private static string GetPostgresConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration
            .GetSection(InfrastructureOptions.SectionName)
            .GetValue<string>(nameof(InfrastructureOptions.ConnectionString));

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException(MissingConnectionStringMessage)
            : connectionString;
    }
}
