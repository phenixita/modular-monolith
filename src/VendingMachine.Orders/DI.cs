using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Orders
{
    public static class DI
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services)
        {
            return services.AddMediatR(typeof(OrderService).Assembly);
        }

    }
}
