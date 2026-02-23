using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Persistence;

namespace VendingMachine.Inventory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInventoryModule(this IServiceCollection services) =>
         services
         .AddScoped<IInventoryRepository>(sp =>
         {
             var options = sp.GetRequiredService<IOptions<InfrastructureOptions>>();
             var connectionString = options.Value.ConnectionString;
             return new PostgresInventoryRepository(connectionString, sp.GetRequiredService<ITransactionContext>());
         })
         .AddScoped<IInventoryService, InventoryService>()
         .AddMediatR(typeof(InventoryService).Assembly);

        public static IServiceCollection AddInventoryModuleForTesting(
            this IServiceCollection services,
            IInventoryRepository repository) =>
            services
            .AddSingleton(repository)
            .AddScoped<IInventoryService, InventoryService>()
            .AddMediatR(typeof(InventoryService).Assembly);


        // public static IServiceCollection WithPostgreStorage(this IServiceCollection services, string connectionString) =>
        //    services.AddScoped<IInventoryRepository>(sp =>
        //             new PostgresInventoryRepository(
        //                 connectionString,
        //                 sp.GetRequiredService<ITransactionContext>()));


    }
}
