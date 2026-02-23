using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Orders
{
    public static class DI
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services)
        {
            services.AddSingleton<IOrdersUnitOfWork, TransactionScopeOrdersUnitOfWork>();
            return services.AddMediatR(typeof(OrderService).Assembly);
        }

    }
}
