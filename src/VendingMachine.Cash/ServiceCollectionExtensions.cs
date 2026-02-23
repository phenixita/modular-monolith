using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VendingMachine.Persistence;

namespace VendingMachine.Cash
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCashRegisterModule(this IServiceCollection services) =>
            services.AddScoped<ICashStorage, PostgresCashStorage>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<InfrastructureOptions>>();
                var connectionString = options.Value.ConnectionString;
                return new PostgresCashStorage(connectionString, sp.GetRequiredService<IPostgresTransactionAccessor>());
            })
            .AddScoped<ICashRegisterService, CashRegisterService>()
            .AddMediatR(typeof(CashRegisterService).Assembly); 
    }
}
