using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Persistence;

namespace VendingMachine.Orders
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services)
        => services.AddScoped<IOrderService, OrderService>()
        .AddMediatR(typeof(OrderService).Assembly);

        public static IServiceCollection AddOrdersModuleForTesting(this IServiceCollection services)
        => services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
        .AddScoped<IOrderService, OrderService>()
        .AddMediatR(typeof(OrderService).Assembly);

        private sealed class NoOpUnitOfWork : IUnitOfWork
        {
            public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
                action(cancellationToken);

            public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
                action(cancellationToken);
        }


    }
}
