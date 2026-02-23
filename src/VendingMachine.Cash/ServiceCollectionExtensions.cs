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
                return new PostgresCashStorage(connectionString, sp.GetRequiredService<ITransactionContext>());
            })
            .AddScoped<ICashRegisterService, CashRegisterService>()
            .AddMediatR(typeof(CashRegisterService).Assembly);

        public static IServiceCollection AddCashRegisterModuleForTesting(
            this IServiceCollection services,
            ICashStorage storage) =>
            services.AddSingleton(storage)
            .AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
            .AddScoped<ICashRegisterService, CashRegisterService>()
            .AddMediatR(typeof(CashRegisterService).Assembly);

        private sealed class NoOpUnitOfWork : IUnitOfWork
        {
            public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
                action(cancellationToken);

            public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
                action(cancellationToken);
        }
    }
}
