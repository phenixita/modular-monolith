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

        public static IServiceCollection AddOrdersModuleForTests(this IServiceCollection services)
        => services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>()
        .AddScoped<IOrderService, OrderService>()
        .AddMediatR(typeof(OrderService).Assembly);

    }
}
