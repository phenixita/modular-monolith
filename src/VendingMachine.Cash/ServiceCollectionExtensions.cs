using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VendingMachine.Persistence;

namespace VendingMachine.Cash
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCashRegisterModule(this IServiceCollection services) =>
            services.AddScoped<ICashRepository, PostgresCashRepository>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<InfrastructureOptions>>();
                var connectionString = options.Value.ConnectionString;
                return new PostgresCashRepository(connectionString, sp.GetRequiredService<ITransactionContext>());
            })
            .AddScoped<ICashRegisterService, CashRegisterService>()
            .AddMediatR(typeof(CashRegisterService).Assembly);

        public static IServiceCollection AddCashRegisterModuleForTests(
            this IServiceCollection services) =>
            services.AddSingleton<ICashRepository, InMemoryCashrepository>()
            .AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
            .AddScoped<ICashRegisterService, CashRegisterService>()
            .AddMediatR(typeof(CashRegisterService).Assembly);
    }
}
