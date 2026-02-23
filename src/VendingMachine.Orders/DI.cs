using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Orders
{
    public static class DI
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services)
        => services.AddScoped<IOrderService, OrderService>()
        .AddMediatR(typeof(OrderService).Assembly);


    }
}
