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

        public static IServiceCollection AddReportingModuleForTests(this IServiceCollection services) =>
            services
                .AddSingleton<IReportingRepository, InMemoryReportingRepository>()
                .AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
                .AddScoped<IReportingService, ReportingService>()
                .AddMediatR(typeof(ReportingService).Assembly);
    }
}
