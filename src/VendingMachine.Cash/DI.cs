using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Cash
{
    public static class DI
    {
        public static IServiceCollection AddCashRegisterModule(this IServiceCollection services)
        {
            return services.AddMediatR(typeof(CashRegisterService).Assembly);
        }

    }
}
