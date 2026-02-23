using Microsoft.Extensions.DependencyInjection;
using __ROOT_NAMESPACE__.__MODULE_NAME__;
using Xunit;

namespace __ROOT_NAMESPACE__.__MODULE_NAME__.Tests.L0;

[Trait("Level", "L0")]
public sealed class __MODULE_NAME__ServiceTests
{
    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var services = new ServiceCollection();
        services.Add__MODULE_NAME__Module();
        services.AddScoped<__MODULE_NAME__Service>();

        await using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<__MODULE_NAME__Service>();

        var result = await service.Ping();

        Assert.Equal("pong", result);
    }
}
