using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VendingMachine.Persistence;
using VendingMachine.Reporting.Abstractions;
using VendingMachine.Reporting.Infrastructure;

namespace VendingMachine.Reporting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReportingModule(this IServiceCollection services) =>
            services
                .AddScoped<IReportingRepository, PostgresReportingRepository>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<InfrastructureOptions>>();
                    var connectionString = options.Value.ConnectionString;
                    return new PostgresReportingRepository(connectionString, sp.GetRequiredService<ITransactionContext>());
                })
                .AddScoped<IReportingService, ReportingService>()
                .AddMediatR(typeof(ReportingService).Assembly);

        public static IServiceCollection AddReportingModuleForTesting(this IServiceCollection services) =>
            services
                .AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
                .AddScoped<IReportingService, ReportingService>()
                .AddMediatR(typeof(ReportingService).Assembly);

        public static IServiceCollection AddReportingModuleForTesting(
            this IServiceCollection services,
            IReportingRepository repository) =>
            services
                .AddSingleton(repository)
                .AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
                .AddScoped<IReportingService, ReportingService>()
                .AddMediatR(typeof(ReportingService).Assembly);

        private sealed class NoOpUnitOfWork : IUnitOfWork
        {
            public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
                action(cancellationToken);

            public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
                action(cancellationToken);
        }
    }
}
