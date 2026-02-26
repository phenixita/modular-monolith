using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Orders;
using VendingMachine.Reporting.Abstractions;
using Xunit;

namespace VendingMachine.Reporting.Tests.L0;

[Trait("Level", "L0")]
public sealed class ReportingServiceTests
{
    [Fact]
    public async Task GetDashboardStats_WhenCalled_ReturnsRepositoryStats()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingModuleForTests();
        var provider = services.BuildServiceProvider();

        // Pre-populate in-memory repository with test data
        var repository = provider.GetRequiredService<IReportingRepository>();
        await repository.RecordOrderAsync("COLA", 2.50m, DateTimeOffset.UtcNow);
        await repository.RecordOrderAsync("SPRITE", 3.00m, DateTimeOffset.UtcNow);

        var service = provider.GetRequiredService<IReportingService>();
        var actual = await service.GetDashboardStatsAsync();

        Assert.Equal(5.50m, actual.TotalRevenue);
        Assert.Equal(2, actual.OrderCount);
        Assert.Equal(2.75m, actual.AverageOrderValue);
    }

    [Fact]
    public async Task PublishOrderConfirmed_WhenCalled_RecordsOrderInReportingRepository()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingModuleForTests();
        var provider = services.BuildServiceProvider();

        var publisher = provider.GetRequiredService<IPublisher>();
        var occurredAt = new DateTimeOffset(2026, 2, 25, 12, 0, 0, TimeSpan.Zero);

        await publisher.Publish(new OrderConfirmed("COLA", 1.50m, occurredAt));

        // Verify order was recorded by querying repository
        var repository = provider.GetRequiredService<IReportingRepository>();
        var stats = await repository.GetDashboardStatsAsync();

        Assert.Equal(1.50m, stats.TotalRevenue);
        Assert.Equal(1, stats.OrderCount);
        Assert.Equal(1.50m, stats.AverageOrderValue);
    }
}
