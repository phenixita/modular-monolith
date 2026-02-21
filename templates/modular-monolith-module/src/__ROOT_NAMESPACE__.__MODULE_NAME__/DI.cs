using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace __ROOT_NAMESPACE__.__MODULE_NAME__;

public static class DI
{
    public static IServiceCollection Add__MODULE_NAME__Module(this IServiceCollection services)
    {
        return services.AddMediatR(typeof(__MODULE_NAME__Service).Assembly);
    }
}
