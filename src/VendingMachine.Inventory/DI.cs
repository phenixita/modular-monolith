using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VendingMachine.Inventory
{
    public static class DI
    {
        public static IServiceCollection AddVendingMachineInventoryModule(this IServiceCollection services)
        {
            return services.AddMediatR(typeof(InventoryService).Assembly);
        }

    }
}
